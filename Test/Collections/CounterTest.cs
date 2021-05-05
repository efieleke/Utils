using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class CounterTest
    {
        [TestMethod]
        public void BasicTest()
        {
            var names = new Counter<string>(2);
            names.Increment("James");
            names.Increment("James");
            names.Increment("Kathy", 3);

            Assert.AreEqual(2, names.Count);
            Assert.AreEqual(2, names.CountOf("James"));
            Assert.AreEqual(3, names["Kathy"]);
            Assert.AreEqual(0, names["George"]);

            var counts = new Dictionary<string, int>(2);

            foreach (KeyValuePair<string, int> entry in names)
            {
                counts.Add(entry.Key, entry.Value);
            }

            Assert.AreEqual(2, counts.Count);

            foreach (KeyValuePair<string, int> entry in counts)
            {
                Assert.AreEqual(entry.Value, names.CountOf(entry.Key));
            }

            Assert.AreEqual(2, names.Keys.Count);
            foreach (string name in names.Keys)
            {
                Assert.IsTrue(counts.ContainsKey(name));
            }

            foreach (string name in counts.Keys)
            {
                Assert.IsTrue(names.Keys.Contains(name));
            }

            names.Clear();
            Assert.AreEqual(0, names.Count);
            Assert.AreEqual(0, names.CountOf("James"));
            Assert.AreEqual(0, names["Kathy"]);
        }

        private class FirstLetterEqual : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) => x?[0] == y?[0];
            public int GetHashCode(string obj) => obj[0].GetHashCode();
        }

        [TestMethod]
        public void TestComparer()
        {
            var names = new Counter<string>(new FirstLetterEqual(), 0, new[] { "John", "Jacob", "Charles", "Nancy", "Ned" });
            Assert.AreEqual(3, names.Count);
            Assert.AreEqual(2, names.CountOf("Jolly"));
            Assert.AreEqual(0, names["Kathy"]);
            Assert.AreEqual(2, names["Ned"]);
            Assert.AreEqual(1, names["Chap"]);
            IReadOnlyCollection<string> keys = names.Keys;
            Assert.AreEqual(3, keys.Count);
            Assert.IsTrue(keys.Contains("John"));
            Assert.IsTrue(keys.Contains("Charles"));
            Assert.IsTrue(keys.Contains("Nancy"));
        }
    }
}
