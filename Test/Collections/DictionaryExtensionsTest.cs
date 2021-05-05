using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Collections.Test
{
    [TestClass]
    class DictionaryExtensionsTest
    {
        [TestMethod]
        public void EnumerateCommonKeysTest()
        {
            var dict1 = new Dictionary<string, int>
            {
                {"common_1", 5},
                { "dict1Only_1", 6},
                { "common_2", 7},
                { "dict1Only_2", 8}
            };

            var dict2 = new Dictionary<string, int>
            {
                {"common_1", 9},
                { "dict2Only_1", 10},
                { "common_2", 11},
                { "dict2Only_2", 12}
            };

            Assert.AreEqual(2, dict1.EnumerateCommonKeys(dict2).Count());

            foreach (var entry in dict1.EnumerateCommonKeys(dict2))
            {
                switch (entry.Key)
                {
                    case "common_1":
                        Assert.AreEqual(5, entry.Value.Item1);
                        Assert.AreEqual(9, entry.Value.Item2);
                        break;
                    case "common_2":
                        Assert.AreEqual(7, entry.Value.Item1);
                        Assert.AreEqual(11, entry.Value.Item2);
                        break;
                    default:
                        Assert.Fail($"Key {entry.Key} is not common to both dictionaries");
                        break;
                }
            }
        }

        [TestMethod]
        public void CopyTest()
        {
            var dict = new Dictionary<int, int>
            {
                {1, 3},
                {7, 2},
                {4, 3}
            };

            Dictionary<int, int> copy = dict.Copy();
            Assert.AreEqual(dict.Count, copy.Count);

            foreach (var entry in dict)
            {
                Assert.AreEqual(copy[entry.Key], entry.Value);
            }
        }
    }
}
