using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sayer.Sort;
using Sayer.Collections;

namespace Sayer.Sort.Test
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void IndexOfSortedTest()
        {
            Assert.AreEqual(-1, new int[0].GetSubList(0, int.MaxValue).IndexOfSorted(0));
            Assert.AreEqual(-1, new[] { 3 }.GetSubList(0, int.MaxValue).IndexOfSorted(0));
            Assert.AreEqual(0, new[] { 3 }.GetSubList(0, int.MaxValue).IndexOfSorted(3));

            List<int> list = Enumerable.Range(0, 100).ToList();
            IReadOnlyList<int> forTest = list.GetSubList(0, int.MaxValue);

            for (int i = 0; i < forTest.Count; ++i)
            {
                Assert.AreEqual(i, forTest.IndexOfSorted(i));
            }

            Assert.AreEqual(~forTest.Count, forTest.IndexOfSorted(100));
            list.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));

            for (int i = 0; i < forTest.Count; ++i)
            {
                int number = forTest[i];
                Assert.AreEqual(i, forTest.IndexOfSorted(0, forTest.Count, number, Comparer<int>.Create((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal))));
            }

            Assert.AreEqual(-1, forTest.IndexOfSorted(0, forTest.Count, -7, Comparer<int>.Create((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal))));
            list = Enumerable.Range(0, 3).ToList();
            forTest = list.GetSubList(0, int.MaxValue);

            for (int i = 0; i < forTest.Count; ++i)
            {
                Assert.AreEqual(i, forTest.IndexOfSorted(i));
            }

            Assert.AreEqual(~forTest.Count, forTest.IndexOfSorted(3));
        }

        [TestMethod]
        public void UpperAndLowerBoundTest()
        {
            var numbers = new[] { 1, 3, 5, 5, 7, 8 };

            Assert.AreEqual(0, numbers.UpperBound(0)); // 1 > 0
            Assert.AreEqual(1, numbers.UpperBound(1)); // 3 > 1
            Assert.AreEqual(1, numbers.UpperBound(2)); // 3 > 2
            Assert.AreEqual(2, numbers.UpperBound(3)); // 5 > 3
            Assert.AreEqual(2, numbers.UpperBound(4)); // 5 > 4
            Assert.AreEqual(4, numbers.UpperBound(5)); // 7 > 5
            Assert.AreEqual(4, numbers.UpperBound(6)); // 7 > 6
            Assert.AreEqual(5, numbers.UpperBound(7)); // 8 > 7
            Assert.AreEqual(6, numbers.UpperBound(8)); // none > 8
            Assert.AreEqual(6, numbers.UpperBound(9)); // none > 9

            Assert.AreEqual(0, numbers.LowerBound(0)); // 1 >= 0
            Assert.AreEqual(0, numbers.LowerBound(1)); // 1 >= 1
            Assert.AreEqual(1, numbers.LowerBound(2)); // 3 >= 2
            Assert.AreEqual(1, numbers.LowerBound(3)); // 3 >= 3
            Assert.AreEqual(2, numbers.LowerBound(4)); // 5 >= 4
            Assert.AreEqual(2, numbers.LowerBound(5)); // 5 >= 5
            Assert.AreEqual(4, numbers.LowerBound(6)); // 7 >= 6
            Assert.AreEqual(4, numbers.LowerBound(7)); // 7 >= 7
            Assert.AreEqual(5, numbers.LowerBound(8)); // 8 >= 8
            Assert.AreEqual(6, numbers.LowerBound(9)); // none >= 9

            Assert.AreEqual(5, numbers.UpperBound(2, 4, 7));
            Assert.AreEqual(2, numbers.UpperBound(2, 0, 5));

            Assert.AreEqual(4, numbers.LowerBound(2, 4, 7, Comparer<int>.Default));
            Assert.AreEqual(2, numbers.LowerBound(2, 0, 5, Comparer<int>.Default));

            numbers = new int[0];
            Assert.AreEqual(0, numbers.UpperBound(-1)); // none > -1
            Assert.AreEqual(0, numbers.LowerBound(-1)); // none >= -1

            numbers = new[] { 3, 3, 3, 3, 3, 3 };
            Assert.AreEqual(0, numbers.UpperBound(2)); // 3 > 2
            Assert.AreEqual(numbers.Length, numbers.UpperBound(3)); // none > 3
            Assert.AreEqual(0, numbers.LowerBound(2)); // 3 >= 2
            Assert.AreEqual(0, numbers.LowerBound(3)); // 3 >= 3
            Assert.AreEqual(numbers.Length, numbers.LowerBound(4)); // none >= 4
        }

        [TestMethod]
        public void BinarySearchStressTest()
        {
            var random = new Random();
            foreach (var _ in Enumerable.Range(0, 10))
            {
                var list = new List<int>(1000);

                foreach (var __ in Enumerable.Range(0, 1000))
                {
                    int number = random.Next(2, 10);

                    if (number % 2 == 1)
                    {
                        ++number;
                    }

                    list.Add(number);
                }

                list.Sort();
                IReadOnlyList<int> forTesting = list.GetSubList(0, list.Count);
                Assert.AreEqual(~list.Count, forTesting.IndexOfSorted(11));
                const int zeroOpposite = ~0;
                Assert.AreEqual(zeroOpposite, forTesting.IndexOfSorted(1));

                void VerifyIsLowerBound(IReadOnlyList<int> items, int index, int searchFor)
                {
                    if (index > 0)
                    {
                        Assert.IsTrue(items[index - 1] < searchFor);
                    }

                    if (index < items.Count)
                    {
                        Assert.IsTrue(items[index] >= searchFor);
                    }
                }

                void VerifyIsUpperBound(IReadOnlyList<int> items, int index, int searchFor)
                {
                    if (index > 0)
                    {
                        Assert.IsTrue(items[index - 1] <= searchFor);
                    }

                    if (index < items.Count)
                    {
                        Assert.IsTrue(items[index] > searchFor);
                    }
                }

                for (int i = 1; i <= 11; ++i)
                {
                    VerifyIsLowerBound(forTesting, forTesting.LowerBound(i), i);
                    VerifyIsUpperBound(forTesting, forTesting.UpperBound(i), i);
                    int match = forTesting.IndexOfSorted(i);

                    if (i % 2 == 0)
                    {
                        Assert.AreEqual(forTesting[match], i);
                    }
                    else
                    {
                        int greaterIndex = ~match;

                        if (i == 11)
                        {
                            Assert.AreEqual(greaterIndex, forTesting.Count);
                        }
                        else
                        {
                            Assert.IsTrue(forTesting[greaterIndex] > i);

                            if (match > 0)
                            {
                                Assert.IsTrue(forTesting[match - 1] < i);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SortTest()
        {
            IList<int> list = new List<int> { 3, 2, 1, 0 };
            list.SortAll((a, b) => a - b);

            for (int i = 0; i < list.Count; ++i)
            {
                Assert.AreEqual(i, list[i]);
            }
        }

        [TestMethod]
        public void MergeSortTest()
        {
            IList<int> list = new List<int>(100);
            var random = new Random();

            for (int i = 0; i < 100; ++i)
            {
                list.Add(random.Next(1, 10));
            }

            var copy = new List<int>(list);
            list.MergeSort((a, b) => a - b);
            copy.Sort();

            for (int i = 0; i < list.Count; ++i)
            {
                Assert.AreEqual(copy[i], list[i]);
            }

            list.Fill(1);
            list.MergeSort((a, b) => a - b);

            list.Clear();
            list.MergeSort((a, b) => a - b);
            list.Add(0);
            list.MergeSort((a, b) => a - b);
            list.AddAll(Enumerable.Range(1, 9));
            list.MergeSort((a, b) => a - b);

            foreach (int i in Enumerable.Range(0, 10))
            {
                Assert.AreEqual(i, list[i]);
            }

            list.Reverse();
            list.MergeSort((a, b) => a - b);

            foreach (int i in Enumerable.Range(0, 10))
            {
                Assert.AreEqual(i, list[i]);
            }
        }
    }
}
