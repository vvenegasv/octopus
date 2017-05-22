using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Octopus
{
    public class OctopusCollection<TModel, TIdentity> : IProducerConsumerCollection<TModel>, IEnumerable<TModel>, ICollection, IEnumerable where TIdentity : struct
    {
        private readonly IDictionary<string, Type> _uniqueFields;
        private readonly Expression<Func<TModel, TIdentity>> _identityField;
        private Object _syncRoot;
        ICollection<TModel> _items;

        public OctopusCollection()
        {
            _uniqueFields = new Dictionary<string, Type>();
            _syncRoot = new Object();
            _identityField = null;
            _items = new List<TModel>();
        }

        protected OctopusCollection(ICollection<TModel> collection)
        {
            _uniqueFields = new Dictionary<string, Type>();
            _syncRoot = new Object();
            _identityField = null;
            _items = collection;
        }

        public OctopusCollection(Expression<Func<TModel, TIdentity>> identityField) : base()
        {
            _uniqueFields = new Dictionary<string, Type>();
            _syncRoot = new Object();
            _identityField = identityField;
            _items = new List<TModel>();
        }

        public OctopusCollection(ICollection<TModel> collection, Expression<Func<TModel, TIdentity>> identityField) : base()
        {
            _uniqueFields = new Dictionary<string, Type>();
            _syncRoot = new Object();
            _identityField = identityField;
            _items = collection;
        }

        public void Add(TModel item)
        {
            TryAdd(item);
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _items.Clear();
            }
        }

        public bool Contains(TModel item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(TModel[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public bool Remove(TModel item)
        {
            if (!_items.Any())
                return false;

            lock (_syncRoot)
            {
                return _items.Remove(item);
            }
        }

        public bool TryAdd(TModel item)
        {
            PropertyInfo prop = null;
            if (_identityField != null)
                prop = GetPropertyInfo(item, _identityField);


            lock (_syncRoot)
            {
                if (CanAddElement(item))
                {
                    if (prop != null)
                        prop.SetValue(item, _items.Count() + 1);
                    _items.Add(item);
                    return true;
                }
            }

            return false;
        }

        public bool TryTake(out TModel item)
        {
            if (_items.Any())
            {
                item = Activator.CreateInstance<TModel>();
                return false;
            }

            lock (_syncRoot)
            {
                item = _items.FirstOrDefault();
                _items.Remove(item);
                return true;
            }
        }

        public TModel[] ToArray()
        {
            return _items.ToArray();
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

        public IEnumerator<TModel> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
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
    }
}
