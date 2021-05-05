using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class OrderedDictionaryTest
    {
        [TestMethod]
        public void TestFirstAndLast()
        {
            var dict = new OrderedDictionary<int, int>
            {
                new KeyValuePair<int, int>(0, 0), {1, 1}, {2, 2}
            };

            Assert.AreEqual(0, dict.First.Key);
            Assert.AreEqual(0, dict.First.Value);
            Assert.AreEqual(2, dict.Last.Key);
            Assert.AreEqual(2, dict.Last.Value);

            dict = new OrderedDictionary<int, int>(2, OrderedDictionary<int, int>.AddBehavior.AddFirst);

            for (int i = 0; i < 3; ++i)
            {
                dict.Add(i, i);
            }

            Assert.AreEqual(2, dict.First.Key);
            Assert.AreEqual(2, dict.First.Value);
            Assert.AreEqual(0, dict.Last.Key);
            Assert.AreEqual(0, dict.Last.Value);
        }

        [TestMethod]
        public void TestContainsKey()
        {
            var dict = new OrderedDictionary<int, int>();
            Assert.IsFalse(dict.IsReadOnly);

            for (int i = 0; i < 3; ++i)
            {
                Assert.IsFalse(dict.ContainsKey(i));
                Assert.IsFalse(dict.Contains(new KeyValuePair<int, int>(i, i)));
                Assert.IsFalse(dict.TryGetValue(i, out _));

                try
                {
                    int _ = dict[i];
                    Assert.Fail("Did not expect to get here");
                }
                catch (KeyNotFoundException)
                {
                }

                dict.Add(i, i);
                Assert.IsTrue(dict.ContainsKey(i));
                Assert.IsTrue(dict.TryGetValue(i, out _));
                Assert.AreEqual(i, dict[i]);
                Assert.IsTrue(dict.Contains(new KeyValuePair<int, int>(i, i)));
                Assert.IsFalse(dict.Contains(new KeyValuePair<int, int>(i, 5)));
            }
        }

        [TestMethod]
        public void TestOrdering()
        {
            var dict = new OrderedDictionary<int, int>(3);
            int i;

            for (i = 0; i < 3; ++i)
            {
                dict.Add(i, i);
            }

            Assert.AreEqual(3, dict.Keys.Count);
            Assert.AreEqual(3, dict.Values.Count);
            i = 0;

            foreach (int key in dict.Keys)
            {
                Assert.AreEqual(i++, key);
            }

            i = 0;

            foreach (int value in dict.Values)
            {
                Assert.AreEqual(i++, value);
            }

            i = 0;

            foreach (var entry in dict)
            {
                Assert.AreEqual(i, entry.Key);
                Assert.AreEqual(i++, entry.Value);
            }

            dict = new OrderedDictionary<int, int>(3, OrderedDictionary<int, int>.AddBehavior.AddFirst);

            for (i = 0; i < 3; ++i)
            {
                dict.Add(i, i);
            }

            Assert.AreEqual(3, dict.Keys.Count);
            Assert.AreEqual(3, dict.Values.Count);
            i = 2;

            foreach (int key in dict.Keys)
            {
                Assert.AreEqual(i--, key);
            }

            i = 2;

            foreach (int value in dict.Values)
            {
                Assert.AreEqual(i--, value);
            }

            i = 2;

            foreach (var entry in dict)
            {
                Assert.AreEqual(i, entry.Key);
                Assert.AreEqual(i--, entry.Value);
            }

            IEnumerator enumerator = ((IEnumerable)dict).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
        }
        [TestMethod]
        public void TestKeysAndValues()
        {
            var dict = new OrderedDictionary<int, int>();

            for (int i = 2; i < 7; ++i)
            {
                dict.Add(i, i);
            }

            ICollection<int> keys = dict.Keys;
            ICollection<int> values = dict.Values;
            Assert.IsTrue(keys.IsReadOnly);
            Assert.IsTrue(values.IsReadOnly);
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

            var array = new int[2];

            try
            {
                values.CopyTo(array, -1);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                values.CopyTo(array, int.MaxValue);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                values.CopyTo(array, 0);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentException)
            {
            }

            array = new int[7];
            values.CopyTo(array, 1);
            Assert.AreEqual(0, array[0]);
            Assert.AreEqual(0, array[6]);
            Assert.IsTrue(array.Contains(2));
            Assert.IsTrue(array.Contains(3));
            Assert.IsTrue(array.Contains(4));
            Assert.IsTrue(array.Contains(5));
            Assert.IsTrue(array.Contains(6));

            try
            {
                keys.Add(11);
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                keys.Remove(1);
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                keys.Clear();
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                values.Add(11);
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                values.Remove(1);
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                values.Clear();
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
            }
        }
    }
}
