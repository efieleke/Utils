using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sayer.Strings;

namespace Sayer.FileBackedCollections.Test
{
    [TestClass]
    public class FileBackedListTest
    {
        [TestMethod]
        public void BasicTest()
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var typeIO = new TypeIO();

                var types = new[]
                {
                    typeof(int),
                    typeof(string),
                    typeof(List<string>),
                    typeof(double),
                    typeof(uint),
                    typeof(Dictionary<string, int>),
                    typeof(List<int>),
                    typeof(Dictionary<int, string>),
                    typeof(byte),
                    typeof(FileBackedListTest)
                };

                using (var list = new FileBackedList<Type>(fileName, FileMode.Create, 2, typeIO))
                {
                    Assert.IsNull(list.LoadMetaData());
                    list.SaveMetaData("MetaData");
                    Assert.AreEqual("MetaData", list.LoadMetaData());
                    Assert.IsFalse(list.IsReadOnly);
                    list.Clear(); // should be safe on empty list

                    for (int i = 0; i < 10; ++i)
                    {
                        list.Add(types[i]);
                        Assert.IsTrue(list.Count == i + 1);
                        Assert.AreEqual(types[i], list[i]);
                        Assert.AreEqual(i, list.IndexOf(types[i]));
                        Assert.IsTrue(list.Contains(types[i]));
                    }

                    Assert.AreEqual("MetaData", list.LoadMetaData());
                    Assert.IsFalse(list.Contains(typeof(float)));

                    Assert.IsTrue(list.Remove(types[9]));
                    Assert.AreEqual(9, list.Count);

                    for (int i = 0; i < types.Length - 1; ++i)
                    {
                        Assert.AreEqual(list[i], types[i]);
                    }

                    Assert.IsTrue(list.Remove(types[0]));
                    Assert.AreEqual(8, list.Count);
                    Assert.IsFalse(list.Contains(types[0]));

                    for (int i = 1; i < types.Length - 1; ++i)
                    {
                        Assert.AreEqual(list[i - 1], types[i]);
                    }

                    Assert.IsFalse(list.Remove(typeof(float)));
                    Assert.AreEqual(8, list.Count);

                    Assert.IsTrue(list.Remove(types[4]));
                    Assert.AreEqual(7, list.Count);

                    for (int i = 1; i < 4; ++i)
                    {
                        Assert.AreEqual(list[i - 1], types[i]);
                    }

                    for (int i = 5; i < types.Length - 1; ++i)
                    {
                        Assert.AreEqual(list[i - 2], types[i]);
                    }

                    list.Add(types[9]);
                    Assert.AreEqual(8, list.Count);
                    list.Insert(0, types[0]);
                    Assert.AreEqual(9, list.Count);
                    list.Insert(4, types[4]);
                    Assert.AreEqual(10, list.Count);

                    for (int i = 0; i < types.Length - 1; ++i)
                    {
                        Assert.AreEqual(list[i], types[i]);
                    }

                    for (int i = 0; i < types.Length - 1; ++i)
                    {
                        list[list.Count - (i + 1)] = types[i];
                        Assert.AreEqual(10, list.Count);
                        Assert.AreEqual(list[list.Count - (i + 1)], types[i]);
                    }

                    try
                    {
                        list[10] = typeof(string);
                        Assert.Fail("Should have thrown");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }

                    try
                    {
                        list.Insert(11, typeof(string));
                        Assert.Fail("Should have thrown");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }

                    list.Insert(10, typeof(string));
                    Assert.AreEqual(typeof(string), list[10]);

                    list.RemoveAt(10);
                    Assert.AreEqual(10, list.Count);

                    list.Rebuild(10);

                    for (int i = 0; i < types.Length - 1; ++i)
                    {
                        list[list.Count - (i + 1)] = types[i];
                        Assert.AreEqual(10, list.Count);
                        Assert.AreEqual(list[list.Count - (i + 1)], types[i]);
                    }

                    var copy = new List<Type>(list);
                    Assert.AreEqual(copy.Count, list.Count);

                    for (int i = 0; i < types.Length; ++i)
                    {
                        Assert.AreEqual(list[i], copy[i]);
                    }

                    list.Clear();
                    Assert.AreEqual(0, list.Count);

                    foreach (Type type in types)
                    {
                        list.Add(type);
                    }
                }

                using (var list = new FileBackedList<Type>(fileName, FileMode.Open, 5, typeIO))
                {
                    Assert.AreEqual(10, list.Count);

                    for (int i = 0; i < types.Length; ++i)
                    {
                        Assert.AreEqual(types[i], list[i]);
                    }

                    list.Clear();
                    Assert.AreEqual(0, list.Count);
                    Assert.IsFalse(list.Contains(types[0]));
                    Assert.AreEqual(-1, list.IndexOf(types[0]));
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
            string fileName = Path.GetTempFileName();

            try
            {
                var dictIO = new DictionaryIO<string, string>(new StringIO(), new StringIO());

                using (var list = new FileBackedList<IReadOnlyDictionary<string, string>>(fileName, FileMode.Create, 1, dictIO)
                    {
                        new Dictionary<string, string> {{"foo", "bar"}},
                        new Dictionary<string, string> {{"bar", "foo"}}
                    }
                )
                {
                    var array = new IReadOnlyDictionary<string, string>[1];

                    try
                    {
                        list.CopyTo(array, -1);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }

                    try
                    {
                        list.CopyTo(array, int.MaxValue);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentException)
                    {
                    }

                    try
                    {
                        list.CopyTo(array, 0);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentException)
                    {
                    }

                    array = new IReadOnlyDictionary<string, string>[2];

                    try
                    {
                        list.CopyTo(array, 1);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentException)
                    {
                    }

                    list.CopyTo(array, 0);

                    for (int i = 0; i < list.Count; ++i)
                    {
                        IReadOnlyDictionary<string, string> a = list[i];
                        IReadOnlyDictionary<string, string> b = array[i];

                        foreach (KeyValuePair<string, string> entry in a)
                        {
                            Assert.AreEqual(entry.Value, b[entry.Key]);
                        }
                    }
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestLargeList()
        {
            string fileName = Path.GetTempFileName();

            try
            {
                using (var list = new FileBackedList<string>(fileName, FileMode.Create, 1, new StringIO()))
                {
                    string text = new string('a', 2000);

                    for (int i = 0; i < 25000; ++i)
                    {
                        list.Add($"{i} {text}");
                    }

                    int j = 0;
                    foreach (string item in list)
                    {
                        Assert.IsTrue(item.Substring(0, item.IndexOf(' ')).Parse<int>() == j++);
                    }

                    list.RemoveAt(list.Count - 1);
                    Assert.AreEqual(24999, list.Count);

                    j = 0;
                    foreach (string item in list)
                    {
                        Assert.IsTrue(item.Substring(0, item.IndexOf(' ')).Parse<int>() == j++);
                    }

                    list.RemoveAt(0);
                    Assert.AreEqual(24998, list.Count);

                    j = 1;
                    foreach (string item in list)
                    {
                        Assert.IsTrue(item.Substring(0, item.IndexOf(' ')).Parse<int>() == j++);
                    }

                    list.RemoveAt(10);
                    Assert.AreEqual(24997, list.Count);
                    Assert.AreEqual(12, list[10].Substring(0, 2).Parse<int>());
                    Assert.AreEqual(10, list[9].Substring(0, 2).Parse<int>());
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
