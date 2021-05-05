using System;
using System.Collections.Generic;
using System.Linq;

namespace Sayer.Rand
{
    public static class ListExtensions
    {
        /// <summary>
        /// Shuffles using Knuth Fisher Yates algorithm.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">the list to act upon</param>
        public static void Shuffle<T>(this IList<T> list) => Shuffle(list, ThreadSafeRandom.Get());

        /// <summary>
        /// Shuffles using Knuth Fisher Yates algorithm.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">the list to act upon</param>
        /// <param name="random">Random number generator.</param>
        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            foreach (T _ in list.GetShufflingEnumerable(random)) { }
        }

        /// <summary>
        /// Shuffles using Knuth Fisher Yates algorithm, up to a specified number of elements. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">the list to act upon</param>
        /// <param name="count">The number of elements to shuffle</param>
        public static void PartialShuffle<T>(this IList<T> list, int count) => PartialShuffle(list, count, ThreadSafeRandom.Get());

        /// <summary>
        /// Shuffles using Knuth Fisher Yates algorithm, up to a specified number of elements. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">the list to act upon</param>
        /// <param name="count">The number of elements to shuffle</param>
        /// <param name="random">Random number generator.</param>
        public static void PartialShuffle<T>(this IList<T> list, int count, Random random)
        {
            foreach (T _ in list.GetShufflingEnumerable(random).Take(count)) { }
        }

        /// <summary>
        /// Gets an enumerator that shuffles the list as it enumerates, using the Knuth Fisher Yates algorithm. If enumeration is only partial, the list will only be partially shuffled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">the list to act upon</param>
        /// <param name="random">Random number generator. If null, uses a Random object in TLS.</param>
        /// <param name="startIndex">The index of the list at which to start enumerating (and shuffling). Elements before this index will be left alone.</param>
        /// <returns>
        /// The enumerable.
        /// </returns>
        public static IEnumerable<T> GetShufflingEnumerable<T>(this IList<T> list, Random random = null, int startIndex = 0)
        {
            int i = startIndex;

            while (i < list.Count - 1)
            {
                // Because this routine yields, we could continue on a different thread (after an await by the caller). We therefore
                // acquire the ThreadSafeRandom each time, if the caller didn't provide their own Random.
                int randomIndex = i + (random ?? ThreadSafeRandom.Get()).Next(list.Count - i);

                if (randomIndex != i)
                {
                    T temp = list[i];
                    list[i] = list[randomIndex];
                    list[randomIndex] = temp;
                }

                yield return list[i];
                ++i;
            }

            if (i < list.Count)
            {
                yield return list[i];
            }
        }
    }
}
