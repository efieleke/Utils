using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class PermutationsTest
    {
        [TestMethod]
        public void TestPermutationsWithEmptyData()
        {
            var random = new Random();
            TestPermutations(new List<IList<int>>(0), random);
            TestPermutations(new List<IList<int>> { new List<int>(0) }, random);
            TestPermutations(new List<IList<int>> { new List<int> { 1 }, new List<int>(0), new List<int> { 2 } }, random);
        }

        [TestMethod]
        public void TestPermutationsWithSimpleData()
        {
            var input = new List<IList<int>>(3)
            {
                new List<int> {1, 2, 3}, new List<int> {4, 5}, new List<int> {6, 7, 8}
            };

            var permutations = new Permutations<int>(input);

            int i = 0;

            foreach (var l in permutations)
            {
                switch (i++)
                {
                    case 0:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 1, 4, 6 }));
                        break;
                    case 1:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 1, 4, 7 }));
                        break;
                    case 2:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 1, 4, 8 }));
                        break;
                    case 3:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 1, 5, 6 }));
                        break;
                    case 4:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 1, 5, 7 }));
                        break;
                    case 5:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 1, 5, 8 }));
                        break;
                    case 6:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 2, 4, 6 }));
                        break;
                    case 7:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 2, 4, 7 }));
                        break;
                    case 8:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 2, 4, 8 }));
                        break;
                    case 9:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 2, 5, 6 }));
                        break;
                    case 10:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 2, 5, 7 }));
                        break;
                    case 11:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 2, 5, 8 }));
                        break;
                    case 12:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 3, 4, 6 }));
                        break;
                    case 13:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 3, 4, 7 }));
                        break;
                    case 14:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 3, 4, 8 }));
                        break;
                    case 15:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 3, 5, 6 }));
                        break;
                    case 16:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 3, 5, 7 }));
                        break;
                    case 17:
                        Assert.IsTrue(l.SequenceEqual(new List<int> { 3, 5, 8 }));
                        break;
                    default:
                        Assert.Fail($"Unexpected index {i}");
                        break;
                }
            }

            TestPermutations(input, new Random());
        }

        [TestMethod]
        public void TestPermutationsWithRandomData()
        {
            var random = new Random();

            for (int numberLists = 0; numberLists < 6; ++numberLists)
            {
                var input = new List<IList<int>>(numberLists);
                int value = 0;

                for (int i = 0; i < numberLists; ++i)
                {
                    int numberElements = random.Next(10);
                    var list = new List<int>(numberElements);

                    for (int j = 0; j < numberElements; ++j)
                    {
                        list.Add(++value);
                    }

                    input.Add(list);
                }

                TestPermutations(input, random);
            }
        }

        private void TestPermutations(IList<IList<int>> input, Random random)
        {
            int count = 0;
            var permutations = new Permutations<int>(input);

            foreach (IReadOnlyList<int> permutation in permutations)
            {
                Assert.AreEqual(input.Count(l => l.Count > 0), permutation.Count);
                IReadOnlyList<int> compareTo = permutations[count];
                Assert.AreEqual(compareTo.Count, permutation.Count);

                for (int i = 0; i < compareTo.Count; ++i)
                {
                    Assert.AreEqual(compareTo[i], permutation[i]);
                }

                ++count;
            }

            int expectedPermutations = input.Any(l => l.Count > 0) ? 1 : 0;

            foreach (IList<int> l in input)
            {
                if (l.Count > 0)
                {
                    expectedPermutations *= l.Count;
                }
            }

            Assert.AreEqual(expectedPermutations, count);
            Assert.AreEqual(expectedPermutations, permutations.Count);

            foreach (IReadOnlyList<int> permutation in new Permutations<int>(input).GetShufflingEnumerable(random))
            {
                Assert.AreEqual(input.Count(l => l.Count > 0), permutation.Count);
            }

            Assert.AreEqual(expectedPermutations, new Permutations<int>(input).GetShufflingEnumerable(random).Count());
            List<IReadOnlyList<int>> allPermutations = permutations.ToList();
            List<IReadOnlyList<int>> shuffledPermutations = new Permutations<int>(input).GetShufflingEnumerable(random).ToList();

            foreach (IReadOnlyList<int> permutation in allPermutations)
            {
                Assert.IsTrue(shuffledPermutations.Any(l => l.SequenceEqual(permutation)));
            }
        }
    }
}
