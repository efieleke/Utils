using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class PoolTest
    {
        [TestMethod]
        public void TestPool()
        {
            using (var pool = new Pool<object>(3, () => new object()))
            {
                TestPool(pool, 3);
            }
        }

        [TestMethod]
        public void TestConcurrentPool()
        {
            using (var pool = new ConcurrentPool<object>(3, () => new object()))
            {
                TestPool(pool, 3);
            }

            using (var pool = new ConcurrentPool<StringBuilder>(
                3,
                () => new StringBuilder("new"), s =>
                {
                    s.Clear();
                    s.Append("existing");
                }))
            {
                var tasks = new Task[10];

                for (int i = 0; i < tasks.Length; ++i)
                {
                    tasks[i] = Task.Run(async () =>
                    {
                        foreach (int j in Enumerable.Range(0, 10000))
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            using (var _ = pool.Borrow())
                            {
                                if (j % 10 == 0)
                                {
                                    await Task.Delay(0);
                                }
                            }
                        }
                    });
                }

                Task.WhenAll(tasks).Wait();

                // There should be exactly three items in the pool
                Assert.AreEqual("existing", pool.Take().ToString());
                Assert.AreEqual("existing", pool.Take().ToString());
                Assert.AreEqual("existing", pool.Take().ToString());
                Assert.AreEqual("new", pool.Take().ToString());
            }
        }

        private void TestPool(IPool<object> pool, int expectedMaxSize)
        {
            Assert.AreEqual(expectedMaxSize, pool.MaxPoolSize);

            object a;
            object b;
            object c;
            object d;

            using (var dBorrowedItem = pool.Borrow())
            using (var cBorrowedItem = pool.Borrow())
            using (var bBorrowedItem = pool.Borrow())
            using (var aBorrowedItem = pool.Borrow())
            {
                a = aBorrowedItem.Value;
                b = bBorrowedItem.Value;
                c = cBorrowedItem.Value;
                d = dBorrowedItem.Value;
                Assert.IsNotNull(a);
            }

            object c1 = pool.Take(); // pool has capacity of 3
            Assert.IsTrue(ReferenceEquals(c, c1));
            object b1 = pool.Take();
            Assert.IsTrue(ReferenceEquals(b, b1));
            object a1 = pool.Take();
            Assert.IsTrue(ReferenceEquals(a, a1));

            pool.Take();
            pool.Take();
            pool.Take();

            pool.Return(d);
            object d1 = pool.Take();
            Assert.IsTrue(ReferenceEquals(d, d1));

            // Pool empty now. Add three d's
            pool.Return(d);
            pool.Return(d);
            pool.Return(d);

            d1 = pool.Take();
            Assert.IsTrue(ReferenceEquals(d, d1));
            d1 = pool.Take();
            Assert.IsTrue(ReferenceEquals(d, d1));
            d1 = pool.Take();
            Assert.IsTrue(ReferenceEquals(d, d1));

            // Pool is empty. This take should result in a new instance.
            d1 = pool.Take();
            Assert.IsFalse(ReferenceEquals(d, d1));
        }
    }
}
