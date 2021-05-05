using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class EnumerableExtensionsTest
    {
        [TestMethod]
        public void MinItemTest()
        {
            var sourceList = new List<Tuple<int, int>>();

            for (int i = 0; i < 100; ++i)
            {
                sourceList.Add(Tuple.Create(i + 1, 100 - i));
            }

            Assert.AreEqual(sourceList[0], sourceList.Minimum(o => o.Item1));
            Assert.AreEqual(sourceList[99], sourceList.Minimum(o => o.Item2));
        }

        [TestMethod]
        public void MaxItemTest()
        {
            var sourceList = new List<Tuple<int, int>>();

            for (int i = 0; i < 100; ++i)
            {
                sourceList.Add(Tuple.Create(i + 1, 100 - i));
            }

            Assert.AreEqual(sourceList[99], sourceList.Maximum(o => o.Item1));
            Assert.AreEqual(sourceList[0], sourceList.Maximum(o => o.Item2));
        }

        [TestMethod]
        public void AtLeastTest()
        {
            var items = new[] { "foo", "bar", "foobar" };

            Assert.IsTrue(new string[0].AtLeast(0));
            Assert.IsFalse(new string[0].AtLeast(1));

            foreach (int i in Enumerable.Range(0, items.Length + 1))
            {
                Assert.IsTrue(items.AtLeast(i));
            }

            Assert.IsFalse(items.AtLeast(4));
            Assert.IsTrue(items.AtLeast(0, s => s.StartsWith("foo")));
            Assert.IsTrue(items.AtLeast(1, s => s.StartsWith("foo")));
            Assert.IsTrue(items.AtLeast(2, s => s.StartsWith("foo")));
            Assert.IsFalse(items.AtLeast(3, s => s.StartsWith("foo")));
            Assert.IsTrue(items.AtLeast(0, s => s.StartsWith("zzz")));
            Assert.IsFalse(items.AtLeast(1, s => s.StartsWith("zzz")));
        }

        private class Name : IComparable<Name>
        {
            internal string First { get; }
            internal string Middle { get; }
            internal string Last { get; }

            internal Name(string first, string middle, string last)
            {
                First = first;
                Middle = middle;
                Last = last;
            }

            public int CompareTo(Name other)
            {
                int result = String.Compare(Last, other.Last, StringComparison.Ordinal);

                if (result == 0)
                {
                    result = String.Compare(First, other.First, StringComparison.Ordinal);

                    if (result == 0)
                    {
                        result = String.Compare(Middle, other.Middle, StringComparison.Ordinal);
                    }
                }

                return result;
            }

            public override bool Equals(object obj)
            {
                return obj is Name name && Equals(name);
            }

            private bool Equals(Name other)
            {
                return string.Equals(First, other.First) && string.Equals(Middle, other.Middle) && string.Equals(Last, other.Last);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (First != null ? First.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Middle != null ? Middle.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Last != null ? Last.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        private class StringLengthComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == null) { throw new ArgumentException(nameof(x)); }
                if (y == null) { throw new ArgumentException(nameof(y)); }
                return x.Length - y.Length;
            }
        }

        [TestMethod]
        public void MinAndMaxTest()
        {
            var andy = new Name("Andrew", "Curtiss", "Fieleke");
            var eric = new Name("Eric", "Norman", "Fieleke");
            var mike = new Name("Michael", "Stephen", "Fieleke");
            var names = new[] { andy, eric, mike };

            // Uses Name.CompareTo()
            Assert.AreEqual(andy, names.Minimum(n => n));
            Assert.AreEqual(mike, names.Maximum(n => n));

            // Uses String.CompareTo() for first name
            Assert.AreEqual(andy, names.Minimum(n => n.First));
            Assert.AreEqual(mike, names.Maximum(n => n.First));

            // Uses String.CompareTo() for middle name
            Assert.AreEqual(andy, names.Minimum(n => n.Middle));
            Assert.AreEqual(mike, names.Maximum(n => n.Middle));

            // Uses String.CompareTo() for last name
            Assert.AreEqual(andy, names.Minimum(n => n.Last)); // if a tie, the first in the collection is returned
            Assert.AreEqual(andy, names.Maximum(n => n.Last)); // if a tie, the first in the collection is returned

            // Uses string length compare for first name
            var lengthCompare = new StringLengthComparer();
            Assert.AreEqual(eric, names.Minimum(n => n.First, lengthCompare));
            Assert.AreEqual(mike, names.Maximum(n => n.First, lengthCompare));

            // Verify we behave well if there is just one element in the enumerable
            Assert.AreEqual(5, new[] { 5 }.Minimum(i => i));
            Assert.AreEqual(5, new[] { 5 }.Maximum(i => i));

            // Verify we fail properly if the enumeration is empty
            try
            {
                new int[] { }.Minimum(i => i);
                Assert.Fail("Did not expect to get here");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                new int[] { }.Maximum(i => i);
                Assert.Fail("Did not expect to get here");
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                EnumerableExtensions.Minimum<int, int>(null, i => i);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                EnumerableExtensions.Maximum<int, int>(null, i => i);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                new int[] { }.Minimum<int, int>(null);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                new int[] { }.Maximum<int, int>(null);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                new int[] { }.Minimum(i => i, null);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                new int[] { }.Maximum(i => i, null);
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public void MultiSetFromListTest()
        {
            List<Tuple<string, int>> entries = new List<Tuple<string, int>>();

            string[] keys = new string[] { "A", "B", "C", "D", "E", "F", "G", "H" };

            foreach (string key in keys)
            {
                for (int i = 1; i < 10; ++i)
                {
                    entries.Add(new Tuple<string, int>(key, i));
                }
            }

            MultiMap<string, Tuple<string, int>> target = entries.ToMultiMap(o => o.Item1);

            Assert.AreEqual(keys.Length, target.Keys.Count);

            foreach (string key in keys)
            {
                Assert.IsTrue(target.ContainsKey(key));
                Assert.AreEqual(9, target[key].Count);

                for (int i = 0; i < 9; ++i)
                {
                    Assert.AreEqual(key, target[key].ElementAt(i).Item1);
                    Assert.AreEqual(i + 1, target[key].ElementAt(i).Item2);
                }
            }
        }
    }
}
