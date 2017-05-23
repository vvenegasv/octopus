using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octopus.Tester.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octopus.Tester.Factories;
using System.Linq;

namespace Octopush.Tester
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestIdentity()
        {
            var data = new OctopushCollection<DummyPerson, int>(x => x.PersonId);

            for (int index = 0; index < 2000; index++)
                data.Add(DummyPersonFactory.Instance.Make());

            //Check if are 2000 unique ids
            Assert.AreEqual(data.Count, data.Select(x => x.PersonId).Distinct().Count());
        }

        [TestMethod]
        public void TestUniqueKey()
        {
            var data = new OctopushCollection<DummyPerson, int>(x => x.PersonId);
            data.AddUniqueField(x => x.BirthDate);
            data.AddUniqueField(x => x.Name);
                        
            for (int index = 0; index < 2000; index++)
                data.Add(DummyPersonFactory.Instance.Make());

            //Try to make a duplicated fields
            var item = data.FirstOrDefault();
            data.Add(DummyPersonFactory.Instance.Clone(item));
            data.Add(DummyPersonFactory.Instance.Clone(item));
            data.Add(DummyPersonFactory.Instance.Clone(item));
            data.Add(DummyPersonFactory.Instance.Clone(item));
            data.Add(DummyPersonFactory.Instance.Clone(item));
              
            //Check if are 2000 unique ids
            Assert.AreEqual(data.Count, data.Select(x => new { x.Name, x.BirthDate }).Distinct().Count());
        }
        

        [TestMethod]
        public void TestRemove()
        {
            var data = new OctopushCollection<DummyPerson, int>(x => x.PersonId);

            for (int index = 0; index < 2000; index++)
                data.Add(DummyPersonFactory.Instance.Make());

            var count = data.Count();

            for(int index = 0; index < 10; index++)
            {
                var item = data.FirstOrDefault();
                data.Remove(item);
            }

            //Check if are 2000 unique ids
            Assert.AreEqual(count - 10, data.Select(x => x.PersonId).Distinct().Count());
        }
    }
}
