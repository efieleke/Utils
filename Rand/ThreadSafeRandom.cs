using System;
using System.Threading;

namespace Sayer.Rand
{
    /// <summary>
    /// Random number generator that is thread-safe.
    /// </summary>
    public static class ThreadSafeRandom
    {
        /// <summary>
        /// Returns a Random instance specific to the current thread. It must not be accessed
        /// by any other thread. It exists in thread local storage.
        /// </summary>
        /// <returns></returns>
        public static Random Get() => _randomTls.Value;

        /// <summary>
        /// Returns the next random double using the Random object in TLS
        /// </summary>
        public static double NextDouble() => Get().NextDouble();

        /// <summary>
        /// Returns the next random int using the Random object in TLS
        /// </summary>
        public static int Next(int minValue, int maxValue) => Get().Next(minValue, maxValue);

        /// <summary>
        /// Returns the next random int using the Random object in TLS
        /// </summary>
        public static int Next(int maxValue) => Get().Next(maxValue);

        /// <summary>
        /// Returns the next random bool using the Random object in TLS
        /// </summary>
        /// <returns></returns>
        public static bool NextBoolean() => Get().Next(2) == 0; // '2' is the exclusive upper bound

        /// <summary>
        /// This should only be called when the executable is exiting. After this method is called,
        /// all other methods of this class will throw an exception if called. It is not necessary
        /// to call this method; it only exists to prevent memory leaks from being reported upon
        /// application shutdown.
        /// </summary>
        public static void Dispose()
        {
            _randomTls.Dispose();
            _randomTls = null;
        }

        private static volatile ThreadLocal<Random> _randomTls = new ThreadLocal<Random>(() =>
        {
            lock (SeedGenerator)
            {
                return new Random(SeedGenerator.Next());
            }
        });

        private static readonly Random SeedGenerator = new Random();
    }
}
