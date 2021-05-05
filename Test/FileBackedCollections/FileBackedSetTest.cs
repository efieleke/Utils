using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.FileBackedCollections.Test
{
    [TestClass]
    public class FileBackedSetTest
    {
        [TestMethod]
        public void BasicTest()
        {
            string fileName = Path.GetTempFileName();

            try
            {
                var stringIO = new StringIO();

                using (var set = new FileBackedSet<string>(fileName, FileMode.Create, 5, stringIO))
                {
                    Assert.IsNull(set.LoadMetaData());
                    set.SaveMetaData("MetaData");
                    Assert.AreEqual("MetaData", set.LoadMetaData());
                    Assert.IsFalse(set.IsReadOnly);
                    set.Clear(); // should be safe on empty set

                    for (int i = 0; i < 10; ++i)
                    {
                        Assert.IsTrue(set.Add(i.ToString()));
                        Assert.IsTrue(set.Count == i + 1);
                        Assert.IsTrue(set.Contains(i.ToString()));
                    }

                    Assert.AreEqual("MetaData", set.LoadMetaData());
                    Assert.IsFalse(set.Contains("10"));
                    Assert.IsTrue(set.Contains("9"));
                    Assert.IsTrue(set.Contains("8"));
                    Assert.IsTrue(set.Contains("7"));
                    Assert.IsTrue(set.Contains("6"));
                    Assert.IsTrue(set.Contains("5"));
                    Assert.IsTrue(set.Contains("4"));
                    Assert.IsTrue(set.Contains("3"));
                    Assert.IsTrue(set.Contains("2"));
                    Assert.IsTrue(set.Contains("1"));
                    Assert.IsTrue(set.Contains("0"));

                    Assert.IsTrue(set.Remove("7"));
                    Assert.AreEqual(9, set.Count);

                    Assert.IsTrue(set.Add("TestEntry"));
                    Assert.IsFalse(set.Add("TestEntry"));
                    Assert.AreEqual(10, set.Count);
                    Assert.IsTrue(set.Contains("5"));
                    Assert.IsTrue(set.Contains("6"));
                    Assert.IsTrue(set.Contains("8"));
                    Assert.IsTrue(set.Contains("9"));
                    Assert.IsTrue(set.Contains("TestEntry"));
                    Assert.IsFalse(set.Contains("7"));

                    Assert.IsTrue(set.Add("TestEntry2"));
                    Assert.IsTrue(set.Contains("5"));
                    Assert.IsTrue(set.Contains("TestEntry2"));

                    Assert.IsTrue(set.Contains("6"));
                    Assert.IsTrue(set.Contains("TestEntry2"));

                    var copy = new HashSet<string>(set);
                    Assert.AreEqual(copy.Count, set.Count);

                    foreach (string entry in set)
                    {
                        Assert.IsTrue(copy.Contains(entry));
                    }

                    set.Rebuild(set.Count * 7);
                    Assert.AreEqual(copy.Count, set.Count);

                    foreach (string entry in set)
                    {
                        Assert.IsTrue(copy.Contains(entry));
                    }
                }

                using (var set = new FileBackedSet<string>(fileName, FileMode.Open, 5, stringIO))
                {
                    Assert.AreEqual(11, set.Count);
                    Assert.IsTrue(set.Contains("5"));
                    Assert.IsTrue(set.Contains("TestEntry2"));
                    set.Clear();
                    Assert.AreEqual(0, set.Count);
                    Assert.IsFalse(set.Contains("6"));
                    Assert.IsFalse(set.Remove("6"));
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
                var intIO = new IntIO();

                using (var set = new FileBackedSet<int>(fileName, FileMode.Create, 3, intIO) { 1, 2, 3 })
                {
                    var array = new int[2];

                    try
                    {
                        set.CopyTo(array, -1);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }

                    try
                    {
                        set.CopyTo(array, int.MaxValue);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentException)
                    {
                    }

                    try
                    {
                        set.CopyTo(array, 0);
                        Assert.Fail("Did not expect to get here");
                    }
                    catch (ArgumentException)
                    {
                    }

                    array = new int[6];
                    set.CopyTo(array, 2);

                    Assert.AreEqual(0, array[0]);
                    Assert.AreEqual(0, array[1]);
                    Assert.AreEqual(0, array[5]);
                    Assert.IsTrue(array.Contains(1));
                    Assert.IsTrue(array.Contains(2));
                    Assert.IsTrue(array.Contains(3));
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
            string fileName = Path.GetTempFileName();

            try
            {
                var intIO = new IntIO();

                using (var set = new FileBackedSet<int>(fileName, FileMode.Create, 2, intIO))
                {
                    Assert.IsFalse(set.Remove(0));
                    set.Add(0);
                    Assert.IsFalse(set.Remove(1));
                    Assert.IsTrue(set.Remove(0));
                    Assert.AreEqual(0, set.Count);
                    set.Add(0);
                    set.Add(1);
                    Assert.IsTrue(set.Remove(0));
                    Assert.IsTrue(set.Remove(1));
                    Assert.AreEqual(0, set.Count);

                    foreach (var _ in set)
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
            string fileName = Path.GetTempFileName();

            try
            {
                var intIO = new IntIO();

                using (var set = new FileBackedSet<int>(fileName, FileMode.Create, 25000, intIO))
                {
                    for (int i = 0; i < 25000; ++i)
                    {
                        set.Add(i);
                    }

                    int count = 0;

                    foreach (var _ in set)
                    {
                        ++count;
                    }

                    Assert.AreEqual(25000, count);

                    count = 0;
                    IEnumerator enumerator = ((IEnumerable)set).GetEnumerator();

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
    }
}
