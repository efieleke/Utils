using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class ConcurrentHashSetTest
    {
        [TestMethod]
        public void TestHashSet()
        {
            var hashSet = new ConcurrentHashSet<int>();
            Assert.AreEqual(0, hashSet.Count);
            Assert.IsFalse(hashSet.Contains(0));

            foreach (int _ in hashSet)
            {
                Assert.Fail("Did not expect to get here");
            }

            Assert.IsTrue(hashSet.Add(0));
            Assert.IsTrue(hashSet.Contains(0));
            Assert.AreEqual(1, hashSet.Count);
            Assert.IsFalse(hashSet.Add(0));
            Assert.AreEqual(1, hashSet.Count);
            Assert.IsTrue(hashSet.Add(10));
            Assert.IsTrue(hashSet.Contains(10));
            Assert.AreEqual(2, hashSet.Count);
            Assert.AreEqual(0, hashSet.Count(i => i == -1));
            Assert.IsTrue(hashSet.Remove(0));
            Assert.AreEqual(1, hashSet.Count);
            Assert.IsFalse(hashSet.Remove(0));
            Assert.AreEqual(1, hashSet.Count);
            hashSet.Clear();
            Assert.AreEqual(0, hashSet.Count);
        }
    }
}
