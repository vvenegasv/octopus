using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Tester.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octopus.Tester.Factories;
using System.Linq;

namespace Octopus.Tester
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestIdentity()
        {
            var data = new OctopusCollection<DummyPerson, int>(x => x.PersonId);
            var tasks = new List<Task>();

            //Make 20 threads, with 100 element each one.
            for (int mainIndex = 0; mainIndex < 20; mainIndex++)
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int index = 0; index < 100; index++)
                        data.TryAdd(DummyPersonFactory.Instance.Make());
                }));
            Task.WaitAll(tasks.ToArray());

            //Check if are 2000 unique ids
            Assert.AreEqual(data.Count, data.Select(x => x.PersonId).Distinct().Count());
        }

        [TestMethod]
        public void TestUniqueKey()
        {
            var data = new OctopusCollection<DummyPerson, int>(x => x.PersonId);
            data.AddUniqueField(x => x.BirthDate);
            data.AddUniqueField(x => x.Name);

            var tasks = new List<Task>();

            //Make 20 threads, with 100 element each one, plus 5 duplicated elements
            for (int mainIndex = 0; mainIndex < 20; mainIndex++)
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int index = 0; index < 100; index++)
                        data.TryAdd(DummyPersonFactory.Instance.Make());

                    //Try to make a duplicated fields
                    var item = data.FirstOrDefault();
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                }));
            Task.WaitAll(tasks.ToArray());

            //Check if are 2000 unique ids
            Assert.AreEqual(data.Count, data.Select(x => new { x.Name, x.BirthDate }).Distinct().Count());
        }

        [TestMethod]
        public void TestWithReadAndAsyncAdds()
        {
            var data = new OctopusCollection<DummyPerson, int>(x => x.PersonId);
            data.AddUniqueField(x => x.BirthDate);
            data.AddUniqueField(x => x.Name);

            var tasks = new List<Task>();

            //Make 20 threads, with 100 element each one, plus 5 duplicated elements
            for (int mainIndex = 0; mainIndex < 20; mainIndex++)
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int index = 0; index < 100; index++)
                        data.TryAdd(DummyPersonFactory.Instance.Make());

                    //Try to make a duplicated fields
                    var item = data.FirstOrDefault();
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));

                    var filtered = data.Where(x => x.BirthDate >= new DateTime(2000, 1, 1) && x.BirthDate <= new DateTime(2010, 1, 1)).ToList();                    
                }));
            Task.WaitAll(tasks.ToArray());

            //Check if are 2000 unique ids
            Assert.AreEqual(data.Count, data.Select(x => new { x.Name, x.BirthDate }).Distinct().Count());
        }


        [TestMethod]
        public void TestWithReadRemoveAndAsyncAdds()
        {
            var data = new OctopusCollection<DummyPerson, int>(x => x.PersonId);
            data.AddUniqueField(x => x.BirthDate);
            data.AddUniqueField(x => x.Name);

            var tasks = new List<Task>();

            //Make 20 threads, with 100 element each one, plus 5 duplicated elements
            for (int mainIndex = 0; mainIndex < 20; mainIndex++)
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int index = 0; index < 100; index++)
                        data.TryAdd(DummyPersonFactory.Instance.Make());

                    //Try to make a duplicated fields
                    var item = data.FirstOrDefault();
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    data.Add(DummyPersonFactory.Instance.Clone(item));
                    
                }));
            Task.WaitAll(tasks.ToArray());

            var filtered = data.Where(x => x.BirthDate >= new DateTime(2000, 1, 1) && x.BirthDate <= new DateTime(2010, 1, 1)).ToList();
            Parallel.ForEach(filtered, filteredItem =>
            {
                data.Remove(filteredItem);
            });

            //Check if are 2000 unique ids
            Assert.AreEqual(data.Count, data.Select(x => new { x.Name, x.BirthDate }).Distinct().Count());
        }
    }
}
