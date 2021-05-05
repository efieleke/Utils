using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void SwapTest()
        {
            var list = new List<int> { 5, 4, 3, 2, 1 };

            foreach (int i in Enumerable.Range(0, 3))
            {
                list.Swap(i, (list.Count - 1) - i);
            }

            foreach (int i in Enumerable.Range(0, list.Count))
            {
                Assert.AreEqual(i + 1, list[i]);
            }
        }

        [TestMethod]
        public void SiftTest()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };

            list.Sift(i => list[i] % 2 == 0);
            Assert.AreEqual(2, list[0]);
            Assert.AreEqual(4, list[1]);
            Assert.AreEqual(list.Count, list.Distinct().Count());

            list.Sift(i => list[i] % 2 == 0);
            Assert.AreEqual(2, list[0]);
            Assert.AreEqual(4, list[1]);
            Assert.AreEqual(list.Count, list.Distinct().Count());

            list.Sift(i => list[i] % 2 == 1);
            Assert.AreEqual(3, list[0]);
            Assert.AreEqual(1, list[1]);
            Assert.AreEqual(5, list[2]);
            Assert.AreEqual(list.Count, list.Distinct().Count());
        }

        private class ListWrapper<T> : IList<T>
        {
            internal ListWrapper()
            {
                _list = new List<T>();
            }

            private readonly List<T> _list;
            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_list).GetEnumerator();
            }

            public void Add(T item)
            {
                _list.Add(item);
            }

            public void Clear()
            {
                _list.Clear();
            }

            public bool Contains(T item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                return _list.Remove(item);
            }

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            public int IndexOf(T item)
            {
                return _list.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                _list.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }

            public T this[int index]
            {
                get => _list[index];
                set => _list[index] = value;
            }
        }

        [TestMethod]
        public void RemoveSpanTest()
        {
            var list = new ListWrapper<int> { 0, 1, 2, 3, 4, 5 };
            list.RemoveSpan(2, 2);
            Assert.AreEqual(4, list.Count);
            Assert.IsTrue(list.Contains(0));
            Assert.IsTrue(list.Contains(1));
            Assert.IsTrue(list.Contains(4));
            Assert.IsTrue(list.Contains(5));
            list.RemoveSpan(2, 2);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Contains(0));
            Assert.IsTrue(list.Contains(1));
            list.RemoveSpan(0, 2);
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void RemoveMatchesTest()
        {
            var list = new ListWrapper<int> { 0, 1, 2, 3, 2, 1 };
            Assert.AreEqual(0, list.RemoveMatches(i => i == 7));
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual(2, list.RemoveMatches(i => i == 2));
            Assert.AreEqual(4, list.Count);
            Assert.IsFalse(list.Contains(2));
            Assert.AreEqual(1, list.RemoveMatches(i => i == 0));
            Assert.AreEqual(3, list.Count);
            Assert.IsFalse(list.Contains(0));
            Assert.AreEqual(2, list.RemoveMatches(i => i == 1));
            Assert.AreEqual(1, list.Count);
            Assert.IsFalse(list.Contains(1));
            Assert.AreEqual(1, list.RemoveMatches(i => i == 3));
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void SiftAsyncTest()
        {
            SiftAsyncTest(4);
            SiftAsyncTest(1);
        }

        private void SiftAsyncTest(int numThreads)
        {
            const int size = 100;
            var list = new List<int>(size);
            list.AddRange(Enumerable.Range(0, size));

            async Task<bool> Matcher(int listIndex)
            {
                await Task.Delay(1);
                return list[listIndex] % 2 == 0;
            }

            int result = list.SiftAsync(Matcher, numThreads).Result;
            Assert.AreEqual(result, list.Count / 2);
            Assert.AreEqual(list.Count, list.Distinct().Count());

            for (int i = 0; i < list.Count / 2; ++i)
            {
                Assert.AreEqual(i * 2, list[i]);
            }

            for (int i = result; i < list.Count; ++i)
            {
                Assert.IsTrue(list[i] % 2 == 1);
            }
        }

        [TestMethod]
        public void FillTest()
        {
            var items = new string[5];
            Assert.IsTrue(items.All(s => s == null));
            items.Fill("foo");
            Assert.IsTrue(items.All(s => s == "foo"));

            items = new string[5];
            Assert.IsTrue(items.All(s => s == null));
            items.Fill("foo", 1, 3);
            Assert.AreEqual(3, items.Count(s => s == "foo"));
            Assert.IsNull(items[0]);
            Assert.IsNull(items[4]);
        }

        [TestMethod]
        public void SplitTest()
        {
            List<int> sourceList = new List<int>();

            for (int i = 0; i < 100; ++i)
            {
                sourceList.Add(i);
            }

            List<IReadOnlyList<int>> target = sourceList.Split(3).ToList();

            Assert.AreEqual(3, target.Count);
            target.ForEach(o => Assert.IsTrue(o.Count == 33 || o.Count == 34));
            int expectedValue = 0;

            foreach (IReadOnlyList<int> subList in target)
            {
                foreach (int value in subList)
                {
                    Assert.AreEqual(expectedValue++, value);
                }
            }
            Assert.AreEqual(100, expectedValue);
            Assert.AreEqual(99, target[2][32]);
            Assert.AreEqual(34, target[0].Count);
        }

        [TestMethod]
        public void MoreSegmentsSplitTest()
        {
            List<int> sourceList = new List<int>();

            for (int i = 0; i < 10; ++i)
            {
                sourceList.Add(i);
            }

            List<IReadOnlyList<int>> target = sourceList.Split(12).ToList();

            Assert.AreEqual(10, target.Count);
            target.ForEach(o => Assert.AreEqual(1, o.Count));

            for (int i = 0; i < 10; ++i)
            {
                Assert.AreEqual(i, target[i][0]);
            }
        }

        [TestMethod]
        public void NonSplitTest()
        {
            List<int> sourceList = new List<int>();

            for (int i = 0; i < 100; ++i)
            {
                sourceList.Add(i);
            }

            List<IReadOnlyList<int>> target = sourceList.Split(1).ToList();

            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(100, target[0].Count);

            for (int i = 0; i < 100; ++i)
            {
                Assert.AreEqual(i, target[0][i]);
            }
        }

        [TestMethod]
        public void InvalidSplitTest()
        {
            List<int> sourceList = new List<int>();

            for (int i = 0; i < 100; ++i)
            {
                sourceList.Add(i);
            }

            try
            {
                foreach (var _ in sourceList.Split(-1))
                {
                    Assert.Fail("Should not have been able to split with a negative parameter");
                }
            }
            catch (ArgumentException) { }

            try
            {
                foreach (var _ in sourceList.Split(0))
                {
                    Assert.Fail("Should not have been able to split into zero segments");
                }
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void CornerCaseSplitTest()
        {
            List<int> sourceList = new List<int>();

            for (int i = 0; i < 45; ++i)
            {
                sourceList.Add(i);
            }

            for (int i = 1; i < 45; ++i)
            {
                List<IReadOnlyList<int>> target = sourceList.Split(i).ToList();
                Assert.AreEqual(i, target.Count);

                List<int> items = new List<int>();
                for (int j = 0; j < i; ++j)
                {
                    items.AddRange(target[j]);
                }

                Assert.AreEqual(45, items.Count);
                for (int j = 0; j < 45; ++j)
                {
                    Assert.AreEqual(j, items[j]);
                }
            }

            sourceList.Clear();
            Assert.AreEqual(0, sourceList.Split(5).Count());
        }

        [TestMethod]
        public void LargeSplitTest()
        {
            List<int> sourceList = new List<int>();

            for (int i = 0; i < 142337; ++i)
            {
                sourceList.Add(i);
            }

            Stopwatch sw = new Stopwatch(); sw.Start();
            List<IReadOnlyList<int>> target = sourceList.Split(10).ToList();
            sw.Stop();

            Assert.AreEqual(10, target.Count);
            Assert.AreEqual(142337, target.Sum(o => o.Count));
            Assert.IsTrue(sw.Elapsed.TotalSeconds < 2);

            sw.Restart();
            target = sourceList.Split(sourceList.Count / 10).ToList();
            sw.Stop();

            Assert.AreEqual(14233, target.Count);
            Assert.AreEqual(142337, target.Sum(o => o.Count));
            Assert.IsTrue(sw.Elapsed.TotalSeconds < 2);
        }

        [TestMethod]
        public void SplitWithLeftoversTest()
        {
            List<int> numbers = Enumerable.Range(0, 100).ToList();

            foreach (int numSegments in Enumerable.Range(1, numbers.Count + 2))
            {
                List<IReadOnlyList<int>> subLists = numbers.Split(numSegments).ToList();
                int expectedSegments = Math.Min(numSegments, numbers.Count);
                Assert.AreEqual(expectedSegments, subLists.Count);
                Assert.AreEqual(numbers.Count, subLists.Select(l => l.Count).Sum());
                int batchSize = numbers.Count / expectedSegments;
                int remainder = numbers.Count % expectedSegments;

                for (int i = 0; i < subLists.Count; ++i)
                {
                    Assert.AreEqual(i < remainder ? batchSize + 1 : batchSize, subLists[i].Count);
                    Assert.IsTrue(subLists[i].Count > 0);
                }

                int expectedValue = 0;

                foreach (IReadOnlyList<int> subList in subLists)
                {
                    foreach (int value in subList)
                    {
                        Assert.AreEqual(expectedValue++, value);
                    }
                }

                Assert.AreEqual(expectedValue, numbers.Count);
            }
        }

        [TestMethod]
        public void SplitDifferentTypesTest()
        {
            Assert.AreEqual(10, Enumerable.Range(0, 10).ToArray().Split(3).Select(l => l.Count).Sum());
            Assert.AreEqual(10, ((IList<int>)Enumerable.Range(0, 10).ToList()).Split(3).Select(l => l.Count).Sum());
            Assert.AreEqual(10, ((IReadOnlyList<int>)Enumerable.Range(0, 10).ToList()).Split(3).Select(l => l.Count).Sum());
        }

        [TestMethod]
        public void GetSubListTest()
        {
            int[] numbers = Enumerable.Range(0, 5).ToArray();
            IReadOnlyList<int> subList = numbers.GetSubList(0, 0);
            Assert.AreEqual(0, subList.Count);
            subList = numbers.GetSubList(5, 0);
            Assert.AreEqual(0, subList.Count);
            subList = numbers.GetSubList(0, 1);
            Assert.AreEqual(1, subList.Count);
            Assert.AreEqual(0, subList[0]);
            subList = numbers.GetSubList(0, int.MaxValue);
            Assert.AreEqual(5, subList.Count);

            for (int i = 0; i < subList.Count; ++i)
            {
                Assert.AreEqual(i, subList[i]);
            }

            subList = numbers.GetSubList(0, 5);
            Assert.AreEqual(5, subList.Count);

            for (int i = 0; i < subList.Count; ++i)
            {
                Assert.AreEqual(i, subList[i]);
            }

            subList = numbers.GetSubList(4, 1);
            Assert.AreEqual(1, subList.Count);
            Assert.AreEqual(4, subList[0]);
            subList = numbers.GetSubList(1, 3);
            Assert.AreEqual(3, subList.Count);

            for (int i = 0; i < subList.Count; ++i)
            {
                Assert.AreEqual(i + 1, subList[i]);
            }
        }

        [TestMethod]
        public void NestedSubListTest()
        {
            IList<int> numbers = Enumerable.Range(0, 5).ToList();
            var subList = numbers.GetSubList(0, int.MaxValue);
            int expectedCount = numbers.Count;
            Assert.AreEqual(expectedCount, subList.Count);

            while (subList.Count > 0)
            {
                subList = subList.GetSubList(1, int.MaxValue);
                Assert.AreEqual(--expectedCount, subList.Count);
            }
        }

        [TestMethod]
        public void IndexOfTest()
        {
            IReadOnlyList<int> numbers = new[] { 0, 0, 0 };
            Assert.AreEqual(0, numbers.IndexOf(n => n == 0));
            Assert.AreEqual(numbers.Count, numbers.IndexOf(n => n == 1));
            Assert.AreEqual(numbers.Count, numbers.IndexOf(n => n == 1));

            IList<int> numbers2 = Enumerable.Range(0, 3).ToList();
            Assert.AreEqual(1, numbers2.IndexOf(n => n == 1));
            Assert.AreEqual(2, numbers2.IndexOf(n => n == 2));
            Assert.AreEqual(0, numbers2.IndexOf(n => n == 0));
        }

        [TestMethod]
        public void IndexOfMinTest()
        {
            IReadOnlyList<int> numbers = new[] { 0, 0, 0 };
            Assert.AreEqual(0, numbers.IndexOfMin());
            numbers = new[] { 2, 1, 0 };
            Assert.AreEqual(2, numbers.IndexOfMin());
            numbers = new[] { 0, 1, 2 };
            Assert.AreEqual(0, numbers.IndexOfMin());
            numbers = new[] { 1, 0, 2 };
            Assert.AreEqual(1, numbers.IndexOfMin());
            numbers = new int[0];

            try
            {
                numbers.IndexOfMin();
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentException)
            {
                // expected
            }
        }

        [TestMethod]
        public void IndexOfMaxTest()
        {
            IReadOnlyList<int> numbers = new[] { 0, 0, 0 };
            Assert.AreEqual(0, numbers.IndexOfMax());
            numbers = new[] { 2, 1, 0 };
            Assert.AreEqual(0, numbers.IndexOfMax());
            numbers = new[] { 0, 1, 2 };
            Assert.AreEqual(2, numbers.IndexOfMax());
            numbers = new[] { 1, 2, 0 };
            Assert.AreEqual(1, numbers.IndexOfMax());
            numbers = new int[0];

            try
            {
                numbers.IndexOfMax();
                Assert.Fail("Did not expect to get here");
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
}
