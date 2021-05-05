Utils
=========

Utils is a C# library of various classes mainly related to collections, caching, file-based collections, sorting, searching, randomizing, and tasks. These include:

Sayer.Collections
  * MultiMap
  * OrderedDictionary
  * SinglyLinkedList
  * Cache and ConcurrentCache
  * Counter
  * IPool, AbstractPool, Pool and ConcurrentPool
  * ConcurrentHashSet
  * Permutations
  * EnumerableExtensions (Minimum, Maximum, AtLeast, ToMultiMap)
  * CollectionExtensions (AsReadOnly)
  * DictionaryExtensions (EnumerateCommonKeys, EnumerateDisjointKeys, Copy, AsReadOnly)
  * ListExtensions (GetSubList, Sift, Split, Swap, IndexOf, IndexOfMinimum, Fill, Reverse, RemoveMatches, RemoveSpan, AddAll, AsReadOnly)
  * NameValueCollectionExtensions

Sayer.FileBackedCollections (fast file-based IO using little memory; these implement standard collection interfaces)
  * FileBackedDictionary and ConcurrentFileBackedDictionary
  * FileBackedList
  * FileBackedSet
  * FileBackedAppendOnlyCollection

Sayer.Rand
  * ThreadSafeRandom
  * ListExtensions (Shuffle, PartialShuffle, GetShufflingEnumerable)

Sayer.Sort
  * ListExtensions (IndexOfSorted, UpperBound, LowerBound, MergeSort, SortAll, SortRange)
  * LinkedListExtensions (InsertSorted)
  * EnumerableExtensions (GetSortedEnumerable)

Sayer.Strings
  * StringExtensions (TryParse, Parse, ToDictionary)
  * CommandLine (Parse)

Sayer.Tasks
  * TaskRunner (runs an arbitrary number of tasks, but with a limit on the number that can be concurrently running at any one moment)

Author
----
Eric Fieleke
