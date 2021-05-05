using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Sayer.Collections.Test
{
    /// <summary>
    ///This is a test class for MultiMap and is intended
    ///to contain all MultiMap Unit Tests
    ///</summary>
    [TestClass]
    public class MultiMapTest
    {
        [TestMethod]
        public void BasicTest()
        {
            BasicTest(new MultiMap<string, string>());
            BasicTest(new MultiMap<string, string, HashSet<string>>());
            BasicTest(new MultiMap<string, string, SortedSet<string>>());
            BasicTest(new MultiMap<string, string, SinglyLinkedList<string>>());
            BasicTest(new MultiMap<string, string, LinkedList<string>>());
        }

        private void BasicTest(IMultiMap<string, string> addressBook)
        {
            addressBook.Add("James", "Office: 02071729427");
            addressBook.Add("James", "Mobile: 07542139746");
            addressBook.Add("Kathy", "Office: 02071729428");
            addressBook.Add("Kathy", "Mobile: 07759436793");

            Assert.IsTrue(addressBook.ContainsKey("James"));
            Assert.IsTrue(addressBook.ContainsKey("Kathy"));
            Assert.IsFalse(addressBook.ContainsKey("George"));

            Assert.IsTrue(addressBook["James"].Contains("Mobile: 07542139746"));
            Assert.IsTrue(addressBook["Kathy"].Contains("Office: 02071729428"));
            Assert.IsFalse(addressBook["Kathy"].Contains("Mobile: 07542139746"));
            Assert.IsFalse(addressBook["James"].Contains("Office: 02071729428"));

            Assert.AreEqual(addressBook["James"].Count, 2);
            Assert.AreEqual(addressBook["Kathy"].Count, 2);

            int nItems = 0;
            foreach (string val in addressBook["James"])
            {
                Assert.IsFalse(addressBook["Kathy"].Contains(val));
                ++nItems;
            }

            Assert.AreEqual(2, nItems);

            Assert.AreEqual(4, addressBook.Values.ToList().Count);
            Assert.IsTrue(addressBook.Values.Contains("Office: 02071729427"));
            Assert.IsTrue(addressBook.Values.Contains("Mobile: 07542139746"));
            Assert.IsTrue(addressBook.Values.Contains("Office: 02071729428"));
            Assert.IsTrue(addressBook.Values.Contains("Mobile: 07759436793"));

            addressBook.Clear();
            Assert.IsFalse(addressBook.ContainsKey("James"));
            Assert.IsFalse(addressBook.ContainsKey("Kathy"));
        }

        [TestMethod]
        public void TestMultiMapRemove()
        {
            TestMultiMapRemove(new MultiMap<string, string>());
            TestMultiMapRemove(new MultiMap<string, string, HashSet<string>>());
            TestMultiMapRemove(new MultiMap<string, string, SortedSet<string>>());
            TestMultiMapRemove(new MultiMap<string, string, SinglyLinkedList<string>>());
            TestMultiMapRemove(new MultiMap<string, string, LinkedList<string>>());
        }

        private void TestMultiMapRemove(IMultiMap<string, string> addressBook)
        {
            addressBook.Add("James", "Office: 02071729427");
            addressBook.Add("James", "Mobile: 07542139746");
            addressBook.Add("Kathy", "Office: 02071729428");
            addressBook.Add("Kathy", "Mobile: 07759436793");

            Assert.IsTrue(addressBook.ContainsKey("James"));
            Assert.IsTrue(addressBook.ContainsKey("Kathy"));

            Assert.AreEqual(2, addressBook.KeyCount);
            Assert.IsTrue(addressBook.Remove("James", "Office: 02071729427"));
            Assert.AreEqual(2, addressBook.KeyCount);
            Assert.IsTrue(addressBook.Remove("Kathy", "Mobile: 07759436793"));
            Assert.AreEqual(2, addressBook.KeyCount);
            Assert.IsFalse(addressBook.Remove("Kathy", "blah"));
            Assert.AreEqual(2, addressBook.KeyCount);
            Assert.IsFalse(addressBook.Remove("George"));

            Assert.IsFalse(addressBook["James"].Contains("Office: 02071729428"));
            Assert.IsTrue(addressBook["James"].Contains("Mobile: 07542139746"));
            Assert.IsTrue(addressBook["Kathy"].Contains("Office: 02071729428"));
            Assert.IsFalse(addressBook["Kathy"].Contains("Mobile: 07542139746"));
            Assert.IsTrue(addressBook.ContainsKey("James"));
            Assert.IsTrue(addressBook.ContainsKey("Kathy"));

            Assert.IsTrue(addressBook.Remove("James", "Mobile: 07542139746"));
            Assert.AreEqual(1, addressBook.KeyCount);
            Assert.IsTrue(addressBook.Remove("Kathy", "Office: 02071729428"));
            Assert.AreEqual(0, addressBook.KeyCount);
            Assert.IsFalse(addressBook.ContainsKey("James"));
            Assert.IsFalse(addressBook.ContainsKey("Kathy"));
        }

        [TestMethod]
        public void TestMultiMapNoDupesRemove()
        {
            TestMultiMapNoDupesRemove(new MultiMap<string, string, HashSet<string>>());
            TestMultiMapNoDupesRemove(new MultiMap<string, string, SortedSet<string>>());
        }

        private void TestMultiMapNoDupesRemove(IMultiMap<string, string> addressBook)
        {
            addressBook.Add("James", "Office: 02071729427");
            addressBook.Add("James", "Mobile: 07542139746");
            addressBook.Add("Kathy", "Office: 02071729428");
            addressBook.Add("Kathy", "Mobile: 07759436793");

            Assert.IsTrue(addressBook.ContainsKey("James"));
            Assert.IsTrue(addressBook.ContainsKey("Kathy"));

            Assert.IsTrue(addressBook.Remove("James", "Office: 02071729427"));
            Assert.IsTrue(addressBook.Remove("Kathy", "Mobile: 07759436793"));
            Assert.IsFalse(addressBook.Remove("Kathy", "blah"));
            Assert.IsFalse(addressBook.Remove("George"));

            Assert.IsFalse(addressBook.ContainsValue("James", "Office: 02071729428"));
            Assert.IsTrue(addressBook.ContainsValue("James", "Mobile: 07542139746"));
            Assert.IsTrue(addressBook.ContainsValue("Kathy", "Office: 02071729428"));
            Assert.IsFalse(addressBook.ContainsValue("Kathy", "Mobile: 07542139746"));
            Assert.IsTrue(addressBook.ContainsKey("James"));
            Assert.IsTrue(addressBook.ContainsKey("Kathy"));

            Assert.IsTrue(addressBook.Remove("James", "Mobile: 07542139746"));
            Assert.IsTrue(addressBook.Remove("Kathy", "Office: 02071729428"));
            Assert.IsFalse(addressBook.ContainsKey("James"));
            Assert.IsFalse(addressBook.ContainsKey("Kathy"));
        }
    }
}
