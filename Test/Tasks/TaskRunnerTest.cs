using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Tasks.Utils
{
    [TestClass]
    public class TaskRunnerTest
    {
        [TestMethod]
        public void TestSuccess()
        {
            TestSuccessAsync().Wait();
        }

        [TestMethod]
        public void TestFailAdd()
        {
            TestFailAddAsync().Wait();
        }

        [TestMethod]
        public void TestFail()
        {
            TestFailAsync().Wait();
        }

        [TestMethod]
        public void TestLambda()
        {
            TestLambdaAsync().Wait();
        }

        private static async Task TestSuccessAsync()
        {
            var incrementor = new Incrementor(false);
            var taskRunner = new TaskRunner(4);

            for (int i = 0; i < 100; ++i)
            {
                await taskRunner.Add(incrementor.Increment()).ConfigureAwait(false);
            }

            await taskRunner.WhenAll().ConfigureAwait(false);

            if (incrementor.Value != 100)
            {
                throw new Exception("Value != 100");
            }
        }

        private static async Task TestFailAddAsync()
        {
            var incrementor = new Incrementor(true);
            var taskRunner = new TaskRunner(4);
            int i = 0;

            while (i < 4)
            {
                try
                {
                    await taskRunner.Add(incrementor.Increment()).ConfigureAwait(false);
                    ++i;
                }
                catch (Exception e)
                {
                    if (e.Message != "Failed" || i != 3)
                    {
                        throw;
                    }

                    break;
                }
            }

            if (i == 4)
            {
                throw new Exception("Succeeded unexpectedly");
            }
        }

        private static async Task TestFailAsync()
        {
            var incrementor = new Incrementor(true);
            var taskRunner = new TaskRunner(4);

            for (int i = 0; i < 3; ++i)
            {
                await taskRunner.Add(incrementor.Increment()).ConfigureAwait(false);
            }

            try
            {
                await taskRunner.WhenAll().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Expected
            }
        }

        private static async Task TestLambdaAsync()
        {
            int count = 0;
            object l = new object();
            var taskRunner = new TaskRunner(4);

            for (int i = 0; i < 100; ++i)
            {
                await taskRunner.Add(async () =>
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    lock (l)
                    {
                        ++count;
                    }
                }).ConfigureAwait(false);
            }

            await taskRunner.WhenAll().ConfigureAwait(false);

            if (count != 100)
            {
                throw new Exception("count != 100");
            }
        }

        private class Incrementor
        {
            internal Incrementor(bool fail)
            {
                _fail = fail;
            }

            internal async Task Increment()
            {
                await Task.Delay(100).ConfigureAwait(false);

                if (_fail)
                {
                    throw new Exception("Failed");
                }

                Interlocked.Increment(ref _value);
            }

            internal int Value => _value;
            private volatile int _value;
            private readonly bool _fail;
        }
    }
}
