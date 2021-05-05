using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.FileBackedCollections.Test
{
    [TestClass]
    public class FileBackedDictionaryTest
    {
        [TestMethod]
        public void BasicTest()
        {
            BasicTestImpl(0);
            BasicTestImpl(1);
            BasicTestImpl(2);
            BasicTestImpl(10000);
        }

        private static void BasicTestImpl(uint cacheSize)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var stringIO = new StringIO();

                using (var dict = new FileBackedDictionary<string, string>(fileName, FileMode.Create, 5, stringIO, stringIO, cacheSize))
                {
                    Assert.IsNull(dict.LoadMetaData());
                    dict.SaveMetaData("MetaData");
                    Assert.AreEqual("MetaData", dict.LoadMetaData());
                    Assert.IsFalse(dict.IsReadOnly);
                    dict.Clear(); // should be safe on empty dictionary

                    for (int i = 0; i < 10; ++i)
                    {
                        dict.Add(i.ToString(), $"Test{i}");
                        Assert.IsTrue(dict.Count == i + 1);
                        Assert.IsTrue(dict.TryGetValue(i.ToString(), out string value));
                        Assert.AreEqual(value, $"Test{i}");
                        Assert.AreEqual(dict[i.ToString()], $"Test{i}");
                    }

                    Assert.AreEqual("MetaData", dict.LoadMetaData());
                    Assert.IsFalse(dict.ContainsKey("10"));
                    Assert.IsTrue(dict.ContainsKey("9"));
                    Assert.IsTrue(dict.ContainsKey("8"));
                    Assert.IsTrue(dict.ContainsKey("7"));
                    Assert.IsTrue(dict.ContainsKey("6"));
                    Assert.IsTrue(dict.ContainsKey("5"));
                    Assert.IsTrue(dict.ContainsKey("4"));
                    Assert.IsTrue(dict.ContainsKey("3"));
                    Assert.IsTrue(dict.ContainsKey("2"));
                    Assert.IsTrue(dict.ContainsKey("1"));
                    Assert.IsTrue(dict.ContainsKey("0"));

                    Assert.IsTrue(dict.Remove("7"));
                    Assert.AreEqual(9, dict.Count);

                    dict.Add(new KeyValuePair<string, string>("TestEntry", "Test"));
                    Assert.IsTrue(dict.ContainsKey("5"));
                    Assert.IsTrue(dict.ContainsKey("6"));
                    Assert.IsTrue(dict.ContainsKey("8"));
                    Assert.IsTrue(dict.ContainsKey("9"));
                    Assert.IsTrue(dict.ContainsKey("TestEntry"));
                    Assert.IsFalse(dict.ContainsKey("7"));
                    Assert.AreEqual(dict["TestEntry"], "Test");
                    dict["TestEntry"] = string.Empty;
                    Assert.AreEqual(dict["TestEntry"], string.Empty);

                    dict.Add("TestEntry2", "Test");
                    Assert.IsTrue(dict.ContainsKey("5"));
                    Assert.IsTrue(dict.ContainsKey("TestEntry2"));

                    try
                    {
                        dict.Add("TestEntry2", "OtherTest");
                        Assert.Fail("Should throw on adding an entry that is already in the dictionary");
                    }
                    catch (ArgumentException)
                    {
                    }

                    Assert.IsTrue(dict.ContainsKey("6"));
                    Assert.IsTrue(dict.ContainsKey("TestEntry2"));

                    var copy = new Dictionary<string, string>(dict);
                    Assert.AreEqual(copy.Count, dict.Count);

                    foreach (KeyValuePair<string, string> entry in dict)
                    {
                        Assert.AreEqual(dict[entry.Key], entry.Value);
                    }

                    dict.Rebuild(dict.Count * 7);
                    Assert.AreEqual(copy.Count, dict.Count);

                    foreach (KeyValuePair<string, string> entry in dict)
                    {
                        Assert.AreEqual(dict[entry.Key], entry.Value);
                    }
                }

                using (var dict = new FileBackedDictionary<string, string>(fileName, FileMode.Open, 5, stringIO, stringIO, cacheSize))
                {
                    Assert.AreEqual(11, dict.Count);
                    Assert.IsTrue(dict.ContainsKey("5"));
                    Assert.AreEqual(dict["TestEntry2"], "Test");
                    dict.Clear();
                    Assert.AreEqual(0, dict.Count);
                    Assert.IsFalse(dict.ContainsKey("6"));
                    Assert.IsFalse(dict.Remove("6"));
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestListIO()
        {
            TestListIOImpl(0);
            TestListIOImpl(1);
            TestListIOImpl(2);
            TestListIOImpl(10000);
        }

        private static void TestListIOImpl(uint cacheSize)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var listIO = new ListIO<string>(new StringIO());

                using (var dict = new FileBackedDictionary<string, IReadOnlyList<string>>(fileName, FileMode.Create, 0, new StringIO(), listIO, cacheSize))
                {
                    dict["Item1"] = new List<string> { "Element1", "Element2", "Element3" };
                    IReadOnlyList<string> value = dict["Item1"];
                    Assert.AreEqual(3, value.Count);
                    Assert.AreEqual("Element1", value[0]);
                    Assert.AreEqual("Element2", value[1]);
                    Assert.AreEqual("Element3", value[2]);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestCopyTo()
        {
            TestCopyToImpl(0);
            TestCopyToImpl(1);
            TestCopyToImpl(2);
            TestCopyToImpl(10000);
        }

        private static void TestCopyToImpl(uint cacheSize)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var intIO = new IntIO();

                using (var dict = new FileBackedDictionary<int, int>(fileName, FileMode.Create, 3, intIO, intIO, cacheSize) { { 1, 1 }, { 2, 2 }, { 3, 3 } })
                {
                    var array = new KeyValuePair<int, int>[2];

                    try
                    {
                        dict.CopyTo(array, -1);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }

                    try
                    {
                        dict.CopyTo(array, int.MaxValue);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentException)
                    {
                    }

                    try
                    {
                        dict.CopyTo(array, 0);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentException)
                    {
                    }

                    array = new KeyValuePair<int, int>[6];
                    dict.CopyTo(array, 2);

                    foreach (var entry in array)
                    {
                        Assert.AreEqual(entry.Key, entry.Value);
                    }

                    Assert.AreEqual(0, array[0].Key);
                    Assert.AreEqual(0, array[0].Value);
                    Assert.AreEqual(0, array[1].Key);
                    Assert.AreEqual(0, array[1].Value);
                    Assert.AreEqual(0, array[5].Key);
                    Assert.AreEqual(0, array[5].Value);
                    Assert.IsTrue(array.Select((e) => e.Key).Contains(1));
                    Assert.IsTrue(array.Select((e) => e.Key).Contains(2));
                    Assert.IsTrue(array.Select((e) => e.Key).Contains(3));
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestRemove()
        {
            TestRemoveImpl(0);
            TestRemoveImpl(1);
            TestRemoveImpl(2);
            TestRemoveImpl(10000);
        }

        private static void TestRemoveImpl(uint cacheSize)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var intIO = new IntIO();

                using (var dict = new FileBackedDictionary<int, int>(fileName, FileMode.Create, 2, intIO, intIO, cacheSize))
                {
                    Assert.IsFalse(dict.Remove(new KeyValuePair<int, int>(0, 0)));
                    dict.Add(0, 0);
                    Assert.IsFalse(dict.Remove(new KeyValuePair<int, int>(1, 0)));
                    Assert.IsFalse(dict.Remove(new KeyValuePair<int, int>(0, 1)));
                    Assert.IsTrue(dict.Remove(new KeyValuePair<int, int>(0, 0)));
                    Assert.AreEqual(0, dict.Count);
                    dict.Add(0, 0);
                    dict.Add(1, 1);
                    Assert.IsTrue(dict.Remove(0));
                    Assert.IsTrue(dict.Remove(1));
                    Assert.AreEqual(0, dict.Count);

                    foreach (var _ in dict)
                    {
                        Assert.Fail("Did not expect to get here");
                    }
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestEnumerate()
        {
            TestEnumerateImpl(0);
            TestEnumerateImpl(1);
            TestEnumerateImpl(2);
            TestEnumerateImpl(10000);
        }

        private static void TestEnumerateImpl(uint cacheSize)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var intIO = new IntIO();

                using (var dict = new FileBackedDictionary<int, int>(fileName, FileMode.Create, 25000, intIO, intIO, cacheSize))
                {
                    for (int i = 0; i < 25000; ++i)
                    {
                        dict.Add(i, i);
                    }

                    int count = 0;

                    foreach (var entry in dict)
                    {
                        Assert.AreEqual(entry.Key, entry.Value);
                        ++count;
                    }

                    Assert.AreEqual(25000, count);

                    count = 0;
                    IEnumerator enumerator = ((IEnumerable)dict).GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        ++count;
                    }

                    Assert.AreEqual(25000, count);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestKeysAndValues()
        {
            TestKeysAndValuesImpl(0);
            TestKeysAndValuesImpl(1);
            TestKeysAndValuesImpl(2);
            TestKeysAndValuesImpl(10000);
        }

        private static void TestKeysAndValuesImpl(uint cacheSize)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var intIO = new IntIO();

                using (var dict = new FileBackedDictionary<int, int>(fileName, FileMode.Create, 25000, intIO, intIO, cacheSize))
                {
                    for (int i = 0; i < 25000; ++i)
                    {
                        dict.Add(i, i);
                    }

                    ICollection<int> keys = dict.Keys;
                    ICollection<int> values = dict.Values;
                    Assert.AreEqual(25000, keys.Count);
                    Assert.AreEqual(keys.Count, values.Count);
                    List<int> expectedNumbers = Enumerable.Range(0, 25000).ToList();

                    using (IEnumerator<int> keyEnum = keys.GetEnumerator())
                    {
                        using (IEnumerator<int> valEnum = values.GetEnumerator())
                        {
                            while (valEnum.MoveNext())
                            {
                                keyEnum.MoveNext();
                                Assert.AreEqual(keyEnum.Current, valEnum.Current);
                                expectedNumbers.Remove(keyEnum.Current);
                            }
                        }
                    }

                    Assert.AreEqual(0, expectedNumbers.Count);


                    Assert.AreEqual(0, expectedNumbers.Count);
                    expectedNumbers = Enumerable.Range(0, 25000).ToList();
                    Assert.IsNotNull(((IEnumerable)values).GetEnumerator());

                    foreach (int val in values)
                    {
                        Assert.IsTrue(expectedNumbers.Remove(val));
                    }

                    Assert.AreEqual(0, expectedNumbers.Count);
                    Assert.IsTrue(values.Contains(0));
                    Assert.IsFalse(values.Contains(25000));
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestItemMissing()
        {
            TestItemMissingImpl(0);
            TestItemMissingImpl(1);
            TestItemMissingImpl(2);
            TestItemMissingImpl(10000);
        }

        private static void TestItemMissingImpl(uint cacheSize)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var stringIO = new StringIO();

                using (var dict = new FileBackedDictionary<string, string>(fileName, FileMode.Create, 5, stringIO, stringIO, cacheSize))
                {
                    try
                    {
                        object _ = dict[""];
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
