using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void BasicTest()
        {
            try
            {
                var _ = new Cache<string, object>(0, null);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentException)
            {
                // expected
            }

            var dict = new Cache<string, object>(5, null);
            Assert.IsFalse(dict.IsReadOnly);
            Assert.AreEqual(5U, dict.MaxSize);
            dict.Clear(); // should be safe on empty dictionary

            for (int i = 0; i < 10; ++i)
            {
                dict.Add(i.ToString(), "Test");
                Assert.IsTrue(dict.Count <= 5);
            }

            Assert.IsFalse(dict.ContainsKey("10"));
            Assert.IsTrue(dict.ContainsKey("9"));
            Assert.IsTrue(dict.ContainsKey("8"));
            Assert.IsTrue(dict.ContainsKey("7"));
            Assert.IsTrue(dict.ContainsKey("6"));
            Assert.IsTrue(dict.ContainsKey("5"));
            Assert.IsFalse(dict.ContainsKey("4"));
            Assert.IsFalse(dict.ContainsKey("3"));
            Assert.IsFalse(dict.ContainsKey("2"));
            Assert.IsFalse(dict.ContainsKey("1"));
            Assert.IsFalse(dict.ContainsKey("0"));

            Assert.IsTrue(dict.Remove("7"));
            Assert.AreEqual(4, dict.Count);

            dict.Add(new KeyValuePair<string, object>("TestEntry", "Test"));
            Assert.IsTrue(dict.ContainsKey("5"));
            Assert.IsTrue(dict.ContainsKey("6"));
            Assert.IsTrue(dict.ContainsKey("8"));
            Assert.IsTrue(dict.ContainsKey("9"));
            Assert.IsTrue(dict.ContainsKey("TestEntry"));
            Assert.IsFalse(dict.ContainsKey("7"));

            dict.Add("TestEntry2", "Test");
            Assert.IsFalse(dict.ContainsKey("5"));
            Assert.IsTrue(dict.ContainsKey("TestEntry2"));

            try
            {
                dict.Add("TestEntry2", "OtherTest");
                Assert.Fail("Should throw on adding an entry that is already in the dictionary");
            }
            catch (ArgumentException) { }

            Assert.IsTrue(dict.ContainsKey("6"));
            Assert.IsTrue(dict.ContainsKey("TestEntry2"));

            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey("6"));
            Assert.IsFalse(dict.Remove("6"));
        }

        [TestMethod]
        public void TestCopyTo()
        {
            IDictionary<int, int> dict = new Cache<int, int>(3, null) { { 1, 1 }, { 2, 2 }, { 3, 3 } };
            var array = new KeyValuePair<int, int>[2];

            try
            {
                dict.CopyTo(array, -1);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                dict.CopyTo(array, int.MaxValue);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                dict.CopyTo(array, 0);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentException)
            {
            }

            array = new KeyValuePair<int, int>[6];
            dict.CopyTo(array, 2);

            foreach (var entry in array)
            {
                Assert.AreEqual(entry.Key, entry.Value);
            }

            Assert.AreEqual(0, array[0].Key);
            Assert.AreEqual(0, array[0].Value);
            Assert.AreEqual(0, array[1].Key);
            Assert.AreEqual(0, array[1].Value);
            Assert.AreEqual(0, array[5].Key);
            Assert.AreEqual(0, array[5].Value);
            Assert.IsTrue(array.Select((e) => e.Key).Contains(1));
            Assert.IsTrue(array.Select((e) => e.Key).Contains(2));
            Assert.IsTrue(array.Select((e) => e.Key).Contains(3));
        }

        [TestMethod]
        public void TestRemove()
        {
            IDictionary<int, int> dict = new Cache<int, int>(2, null);
            Assert.IsFalse(dict.Remove(new KeyValuePair<int, int>(0, 0)));
            dict.Add(0, 0);
            Assert.IsFalse(dict.Remove(new KeyValuePair<int, int>(1, 0)));
            Assert.IsFalse(dict.Remove(new KeyValuePair<int, int>(0, 1)));
            Assert.IsTrue(dict.Remove(new KeyValuePair<int, int>(0, 0)));
            Assert.AreEqual(0, dict.Count);
            dict.Add(0, 0);
            dict.Add(1, 1);
            Assert.IsTrue(dict.Remove(0));
            Assert.IsTrue(dict.Remove(1));
            Assert.AreEqual(0, dict.Count);

            foreach (var _ in dict)
            {
                Assert.Fail("Did not expect to get here");
            }
        }

        [TestMethod]
        public void TestEnumerate()
        {
            IReadOnlyDictionary<int, int> dict = new Cache<int, int>(3, null) { { 0, 0 }, { 1, 1 }, { 2, 2 } };
            int count = 0;

            foreach (var entry in dict)
            {
                Assert.AreEqual(entry.Key, entry.Value);
                ++count;
            }

            Assert.AreEqual(3, count);

            count = 0;
            IEnumerator enumerator = ((IEnumerable)dict).GetEnumerator();

            while (enumerator.MoveNext())
            {
                ++count;
            }

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void TestPruneExpired()
        {
            var dict = new Cache<int, int>(5, null);

            for (int i = 0; i < 5; ++i)
            {
                dict.Add(i, i);
            }

            Assert.AreEqual(5, dict.Count);
            dict.PruneExpired();
            Assert.AreEqual(5, dict.Count);

            dict = new Cache<int, int>(5, TimeSpan.FromTicks(1)) { { 0, 0 } };
            System.Threading.Thread.Sleep(1);
            Assert.AreEqual(1, dict.Count);
            dict.PruneExpired();
            Assert.AreEqual(0, dict.Count);
        }

        [TestMethod]
        public void TestRemoveIf()
        {
            OrderedDictionary<int, int> dict = new Cache<int, int>(5, null);

            for (int i = 0; i < 5; ++i)
            {
                dict.Add(i, i);
            }

            Assert.AreEqual(3, dict.RemoveIf(e => e.Value % 2 == 0));
            Assert.AreEqual(2, dict.Count);
            Assert.IsTrue(dict.ContainsKey(1));
            Assert.IsTrue(dict.ContainsKey(3));
        }

        [TestMethod]
        public void TestGetOrAdd()
        {
            var dict = new Cache<int, List<int>>(5, null);
            Assert.AreEqual(1, dict.GetOrAdd(1, () => new List<int> { 0 }).Count);

            Assert.AreEqual(1, dict.GetOrAdd(
                1,
                () => { Assert.Fail("Didn't expect to get here"); return null; })
                .Count);

            Assert.AreEqual(1, dict.Count);
        }

        [TestMethod]
        public void TestAddOrUpdate()
        {
            OrderedDictionary<int, List<int>> dict = new Cache<int, List<int>>(5, null);

            dict.AddOrUpdate(
                1,
                () => new List<int> { 0 },
                list => { Assert.Fail("Did not expect to get here"); return list; });

            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(1, dict[1].Count);
            Assert.AreEqual(0, dict[1][0]);

            dict.AddOrUpdate(
                1,
                () => { Assert.Fail("Did not expect to get here"); return null; },
                list => { list.Add(1); return list; });

            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(2, dict[1].Count);
            Assert.AreEqual(0, dict[1][0]);
            Assert.AreEqual(1, dict[1][1]);

            Assert.IsFalse(dict.TryAdd(1, null));
            Assert.IsTrue(dict.TryAdd(0, null));
            Assert.AreEqual(2, dict.Count);
            Assert.IsNull(dict[0]);
        }

        [TestMethod]
        public void TestKeysAndValues()
        {
            IDictionary<int, int> dict = new Cache<int, int>(5, null);

            // Will drop first two that were added
            for (int i = 0; i < 7; ++i)
            {
                dict.Add(i, i);
            }

            ICollection<int> keys = dict.Keys;
            ICollection<int> values = dict.Values;
            Assert.AreEqual(5, keys.Count);
            Assert.AreEqual(keys.Count, values.Count);
            var expectedNumbers = new HashSet<int> { 2, 3, 4, 5, 6 };

            using (IEnumerator<int> keyEnum = keys.GetEnumerator())
            {
                using (IEnumerator<int> valEnum = values.GetEnumerator())
                {
                    while (valEnum.MoveNext())
                    {
                        keyEnum.MoveNext();
                        Assert.AreEqual(keyEnum.Current, valEnum.Current);
                        expectedNumbers.Remove(keyEnum.Current);
                    }
                }
            }

            Assert.AreEqual(0, expectedNumbers.Count);
            expectedNumbers = new HashSet<int> { 2, 3, 4, 5, 6 };
            Assert.IsNotNull(((IEnumerable)values).GetEnumerator());

            foreach (int val in values)
            {
                Assert.IsTrue(expectedNumbers.Remove(val));
            }

            Assert.AreEqual(0, expectedNumbers.Count);

            Assert.IsTrue(values.Contains(2));
            Assert.IsFalse(values.Contains(1));
        }

        [TestMethod]
        public void TestExpiration()
        {
            try
            {
                var unused = new Cache<int, int>(uint.MaxValue, TimeSpan.FromTicks(-1));
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentException)
            {
                // expected
            }

            var dict = new Cache<int, int>(uint.MaxValue, TimeSpan.FromMilliseconds(500)) { { 1, 1 }, { 2, 2 }, { 3, 3 } };
            System.Threading.Thread.Sleep(250);
            dict.Add(4, 4);
            Assert.AreEqual(4, dict.Count);
            Assert.IsTrue(dict.ContainsKey(2)); // refreshes expiry for this item
            System.Threading.Thread.Sleep(350);
            dict.Add(5, 5);
            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey(2));
            Assert.IsTrue(dict.ContainsKey(4));
            Assert.IsTrue(dict.ContainsKey(5));

            dict = new Cache<int, int>(uint.MaxValue, null) { { 1, 1 } };
            dict.PruneExpired();
            Assert.AreEqual(1, dict.Count);

            dict = new Cache<int, int>(uint.MaxValue, TimeSpan.FromMilliseconds(500)) { { 1, 1 }, { 2, 2 }, { 3, 3 } };
            System.Threading.Thread.Sleep(250);
            Assert.AreEqual(3, dict.Count);
            Assert.IsTrue(dict.ContainsKey(2)); // refreshes expiry for this item
            System.Threading.Thread.Sleep(350);
            dict.Add(4, 4); // prunes old entries
            Assert.AreEqual(2, dict.Count);
            Assert.IsTrue(dict.ContainsKey(2));
            Assert.IsTrue(dict.ContainsKey(4));

            dict = new Cache<int, int>(uint.MaxValue, TimeSpan.FromMilliseconds(100)) { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 } };
            dict.PruneExpired();
            Assert.AreEqual(5, dict.Count);
            System.Threading.Thread.Sleep(110);
            dict.PruneExpired();
            Assert.AreEqual(0, dict.Count);

            dict = new Cache<int, int>(uint.MaxValue, TimeSpan.FromMilliseconds(100)) { { 1, 1 }, { 2, 2 }, { 3, 3 } };
            System.Threading.Thread.Sleep(50);
            dict.Add(4, 4);
            dict.Add(5, 5);
            System.Threading.Thread.Sleep(60);
            dict.PruneExpired();
            Assert.AreEqual(2, dict.Count);
            Assert.IsTrue(dict.ContainsKey(4));
            Assert.IsTrue(dict.ContainsKey(5));

            dict = new Cache<int, int>(uint.MaxValue, TimeSpan.FromMilliseconds(100)) { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } };
            System.Threading.Thread.Sleep(50);
            dict.Add(5, 5);
            System.Threading.Thread.Sleep(60);
            dict.PruneExpired();
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.ContainsKey(5));
        }

        /// <summary>
        /// Test value refreshing
        /// </summary>
        [TestMethod()]
        public void RefreshTest()
        {
            IDictionary<string, object> dict = new Cache<string, object>(5, null);

            for (int i = 0; i < 5; ++i)
            {
                dict.Add(i.ToString(), "Test");
            }

            object _ = dict["1"];

            for (int i = 5; i < 9; ++i)
            {
                dict.Add(i.ToString(), "Test");
            }

            Assert.IsTrue(dict.ContainsKey("1"));
            Assert.IsTrue(dict.ContainsKey("5"));
            Assert.IsTrue(dict.ContainsKey("6"));
            Assert.IsTrue(dict.ContainsKey("7"));
            Assert.IsTrue(dict.ContainsKey("8"));

            dict["6"] = "Test again";

            for (int i = 9; i < 13; ++i)
            {
                dict.Add(i.ToString(), "Test");
            }

            Assert.IsTrue(dict.ContainsKey("6"));
            Assert.IsTrue(dict.ContainsKey("9"));
            Assert.IsTrue(dict.ContainsKey("10"));
            Assert.IsTrue(dict.ContainsKey("11"));
            Assert.IsTrue(dict.ContainsKey("12"));

            for (int i = 0; i < 5; ++i)
            {
                dict.Add(i.ToString(), "Test");
            }

            Assert.IsFalse(dict.ContainsKey("6"));
            Assert.IsFalse(dict.ContainsKey("9"));
            Assert.IsFalse(dict.ContainsKey("10"));
            Assert.IsFalse(dict.ContainsKey("11"));
            Assert.IsFalse(dict.ContainsKey("12"));
            Assert.IsTrue(dict.ContainsKey("0"));
            Assert.IsTrue(dict.ContainsKey("1"));
            Assert.IsTrue(dict.ContainsKey("2"));
            Assert.IsTrue(dict.ContainsKey("3"));
            Assert.IsTrue(dict.ContainsKey("4"));

            // Test inspection methods affect recent order
            Assert.IsTrue(dict.ContainsKey("1"));
            for (int i = 10; i < 25; ++i)
            {
                Assert.IsTrue(dict.ContainsKey("0"));
                dict.Add(i.ToString(), "Test");
            }
            Assert.IsFalse(dict.ContainsKey("1"));

            Assert.IsTrue(dict.ContainsKey("24"));
            for (int i = 25; i < 50; ++i)
            {
                Assert.IsTrue(dict.TryGetValue("0", out var _));
                Assert.IsFalse(dict.TryGetValue("Englebert", out _));
                dict.Add(i.ToString(), "Test");
            }
            Assert.IsFalse(dict.ContainsKey("24"));

            Assert.IsTrue(dict.ContainsKey("49"));
            for (int i = 50; i < 75; ++i)
            {
                Assert.IsTrue(dict.Contains(new KeyValuePair<string, object>("0", "Test")));
                Assert.IsFalse(dict.Contains(new KeyValuePair<string, object>("0", "Foobar")));
                Assert.IsFalse(dict.Contains(new KeyValuePair<string, object>("Foobar", "Test")));
                dict.Add(i.ToString(), "Test");
            }
            Assert.IsFalse(dict.ContainsKey("49"));
        }

        [TestMethod]
        public void TestItemMissing()
        {
            IReadOnlyDictionary<string, object> dict = new Cache<string, object>(5, null);

            try
            {
                object _ = dict[""];
                Assert.Fail("Did not expect to get here");
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// Verifies Cache doesn't grow beyond limitation when setting values through [] operator
        /// </summary>
        [TestMethod()]
        public void SetValueGrowthTest()
        {
            IDictionary<string, object> dict = new Cache<string, object>(5, null);

            for (int i = 0; i < 1000; ++i)
            {
                dict[i.ToString()] = "Test";
            }

            Assert.AreEqual(5, dict.Count);

            Assert.IsFalse(dict.ContainsKey("0"));
            Assert.IsFalse(dict.ContainsKey("500"));
            Assert.IsFalse(dict.ContainsKey("993"));
            Assert.IsTrue(dict.ContainsKey("995"));
        }
    }
}
