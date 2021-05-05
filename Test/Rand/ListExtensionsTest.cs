using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Rand.Test
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void SortRandomTest()
        {
            List<int> target = new List<int>();

            for (int i = 0; i < 100; ++i)
            {
                target.Add(i);
            }

            target.Shuffle(new Random());

            // Length must be preserved
            Assert.AreEqual(100, target.Count);

            // No duplicates can have been introduced
            HashSet<int> checkDuplicates = new HashSet<int>();
            foreach (int entry in target)
            {
                Assert.IsFalse(checkDuplicates.Contains(entry));
                checkDuplicates.Add(entry);
            }

            // Order must be different
            int totalDist = 0;
            for (int i = 0; i < 100; ++i)
            {
                totalDist += Math.Abs(i - target[i]);
            }
            Assert.IsTrue(totalDist > 0);
        }

        [TestMethod]
        public void ShufflingEnumerateTest()
        {
            const int count = 100;
            var list = new List<int>(count);

            for (int i = 1; i <= 100; ++i)
            {
                list.Add(i);
            }

            var random = new Random();

            list.Shuffle(random);
            Assert.AreEqual(100, list.Count);

            for (int i = 1; i <= 100; ++i)
            {
                Assert.IsTrue(list.Any(value => value == 1));
            }

            for (int i = -1; i < count + 1; ++i)
            {
                if (i > 0 && i <= 100)
                {
                    Assert.AreEqual(i, list.GetShufflingEnumerable(random).First(o => o == i));
                }
                else
                {
                    Assert.IsFalse(list.GetShufflingEnumerable(random).Any(o => o == i));
                }
            }

            list.Clear();
            Assert.IsFalse(list.GetShufflingEnumerable(random).Any(o => o == 1));
            list.Add(1);
            Assert.AreEqual(1, list.GetShufflingEnumerable(random).First(o => o == 1));
            list.Add(2);
            Assert.AreEqual(1, list.GetShufflingEnumerable(random).First(o => o == 1));
            Assert.AreEqual(2, list.GetShufflingEnumerable(random).First(o => o == 2));
        }
    }
}
