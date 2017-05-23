using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Octopush
{
    public class OctopushCollection<TModel, TIdentity> : IList<TModel>, ICollection<TModel>, IList, ICollection, IReadOnlyList<TModel>, IReadOnlyCollection<TModel>, IEnumerable<TModel>, IEnumerable where TIdentity : struct
    {
        private readonly IDictionary<string, Type> _uniqueFields;
        private readonly Expression<Func<TModel, TIdentity>> _identityField;       
        IList<TModel> _items;

        public int Count
        {
            get
            {
                return _items.Count();
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _items.IsReadOnly;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return _items[index];
            }

            set
            {
                _items[index] = (TModel)value;
            }
        }

        public TModel this[int index]
        {
            get
            {
                return _items[index];
            }

            set
            {
                _items[index] = value;
            }
        }

        public OctopushCollection()
        {
            _uniqueFields = new Dictionary<string, Type>();            
            _identityField = null;
            _items = new List<TModel>();
        }

        protected OctopushCollection(ICollection<TModel> collection)
        {
            _uniqueFields = new Dictionary<string, Type>();
            _identityField = null;
            if (collection != null)
                _items = collection.ToList();
            else
                _items = new List<TModel>();
        }

        public OctopushCollection(Expression<Func<TModel, TIdentity>> identityField) : base()
        {
            _uniqueFields = new Dictionary<string, Type>();
            _identityField = identityField;
            _items = new List<TModel>();
        }

        public OctopushCollection(ICollection<TModel> collection, Expression<Func<TModel, TIdentity>> identityField) : base()
        {
            _uniqueFields = new Dictionary<string, Type>();
            _identityField = identityField;
            if (collection != null)
                _items = collection.ToList();
            else
                _items = new List<TModel>();
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            TModel[] typedArray = array as TModel[];
            if (typedArray == null)
                throw new ArgumentException();
            ((ICollection<TModel>)_items).CopyTo(typedArray, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator<TModel> IEnumerable<TModel>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(TModel item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TModel item)
        {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);            
        }

        public void Add(TModel item)
        {
            PropertyInfo prop = null;
            if (_identityField != null)
                prop = GetPropertyInfo(item, _identityField);

            if (CanAddElement(item))
            {
                if (prop != null)
                    prop.SetValue(item, _items.Count() + 1);
                _items.Add(item);            
            }            
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(TModel item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(TModel[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TModel item)
        {
            return _items.Remove(item);
        }


        /// <summary>
        /// Agrega un campo a la clave única de los registros del vector
        /// </summary>
        /// <typeparam name="TKey">Tipo de dato del campo</typeparam>
        /// <param name="keyFieldExpression">Expresión que identifica a la propiedad key de la clase</param>
        public void AddUniqueField<TProperty>(Expression<Func<TModel, TProperty>> keyFieldExpression)
        {
            var defaultModel = Activator.CreateInstance<TModel>();
            var prop = GetPropertyInfo(defaultModel, keyFieldExpression);
            _uniqueFields.Add(prop.Name, typeof(TIdentity));
        }

        private bool CanAddElement(TModel model)
        {
            if (!_uniqueFields.Any())
                return true;

            IQueryable<TModel> query = this.AsQueryable();
            foreach (var uniqueField in _uniqueFields)
            {
                var prop = GetPropertyInfo(model, uniqueField.Key);
                var filter = MakeFilterExpression(prop.Name, prop.GetValue(model));
                query = query.Where(filter);
            }

            return !query.Any();
        }

        private PropertyInfo GetPropertyInfo<TProperty>(TModel source, Expression<Func<TModel, TProperty>> propertyExpression)
        {
            Type type = typeof(TModel);

            MemberExpression member = propertyExpression.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException($"Expression '{propertyExpression.ToString()}' refers to a method, not a property.");

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyExpression.ToString()}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expresion '{propertyExpression.ToString()}' refers to a property that is not from type {type}.");

            return propInfo;
        }

        private PropertyInfo GetPropertyInfo(TModel source, string propertyName)
        {
            return typeof(TModel).GetProperty(propertyName);
        }

        private Expression<Func<TModel, bool>> MakeFilterExpression(string paramName, object value)
        {
            var param = Expression.Parameter(typeof(TModel), "x");
            return Expression.Lambda<Func<TModel, bool>>(
                Expression.Equal(
                    Expression.Property(param, paramName),
                    Expression.Constant(value)
                ), param);
        }

        public int Add(object value)
        {
            PropertyInfo prop = null;
            if (_identityField != null)
                prop = GetPropertyInfo((TModel)value, _identityField);

            if (CanAddElement((TModel)value))
            {
                if (prop != null)
                    prop.SetValue(value, _items.Count() + 1);
                _items.Add((TModel)value);
                return _items.IndexOf((TModel)value);
            }

            return -1;
        }

        public bool Contains(object value)
        {
            return _items.Contains((TModel)value);
        }

        public int IndexOf(object value)
        {
            return _items.IndexOf((TModel)value);
        }

        public void Insert(int index, object value)
        {
            _items.Insert(index, (TModel)value);
        }

        public void Remove(object value)
        {
            _items.Remove((TModel)value);
        }
    }
}
