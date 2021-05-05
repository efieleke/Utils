Utils
=========

C# library related to collections, caching, file-based collections, sorting, searching, randomizing, and tasks. These include:

Collections
  * MultiMap
  * OrderedDictionary
  * SinglyLinkedList
  * Cache
  * Counter
  * Pool
  * ConcurrentHashSet
  * Permutations
  * EnumerableExtensions (Minimum, Maximum, AtLeast, ToMultiMap)
  * CollectionExtensions (AsReadOnly)
  * DictionaryExtensions (EnumerateCommonKeys, EnumerateDisjointKeys, Copy, AsReadOnly)
  * ListExtensions (GetSubList, Sift, Split, Swap, IndexOf, IndexOfMinimum, Fill, Reverse, RemoveMatches, RemoveSpan, AddAll, AsReadOnly)
  * NameValueCollectionExtensions

FileBackedCollections (fast file-based IO using little memory; these implement standard collection interfaces)
  * FileBackedDictionary
  * FileBackedList
  * FileBackedSet
  * FileBackedAppendOnlyCollection

Rand
  * ThreadSafeRandom
  * ListExtensions (Shuffle, PartialShuffle, GetShufflingEnumerable)

Sort
  * ListExtensions (IndexOfSorted, UpperBound, LowerBound, MergeSort, SortAll, SortRange)
  * LinkedListExtensions (InsertSorted)
  * EnumerableExtensions (GetSortedEnumerable)

Strings
  * StringExtensions (TryParse, Parse, ToDictionary)
  * CommandLine (Parse)

Tasks
  * TaskRunner (runs an arbitrary number of tasks, but with a limit on the number that can be concurrently running at any one moment)

Author
----
Eric Fieleke
