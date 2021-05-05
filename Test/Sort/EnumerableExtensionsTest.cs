using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Sort.Test
{
    [TestClass]
    public class EnumerableExtensionsTest
    {
        [TestMethod]
        public void GetSortedEnumerableTest()
        {
            var negativeOdd = new int[] { -7, -5, -3, -1 };
            var negativeEven = new int[] { -8, -6, -4, -2 };
            var positiveOdd = new int[] { 1, 3, 5, 7 };
            var positiveEven = new int[] { 2, 4, 6, 8 };
            var zero = new int[] { 0 };

            List<int> sorted = new[] { negativeOdd, negativeEven, positiveOdd, positiveEven, zero }.GetSortedEnumerable().ToList();
            int index = 0;

            for (int i = -8; i < 9; ++i)
            {
                Assert.AreEqual(i, sorted[index++]);
            }

            Assert.AreEqual(index, sorted.Count);
        }
    }
}
