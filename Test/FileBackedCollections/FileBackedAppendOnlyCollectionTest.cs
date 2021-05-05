using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.FileBackedCollections.Test
{
    [TestClass]
    public class FileBackedAppendOnlyCollectionTest
    {
        [TestMethod]
        public void BasicTest()
        {
            string fileName = Path.GetTempFileName();
            File.Delete(fileName);

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

                using (var coll = new FileBackedAppendOnlyCollection<Type>(fileName, FileMode.CreateNew, typeIO))
                {
                    Assert.AreEqual(false, coll.MetaData);
                    Assert.AreEqual(0, coll.Count);
                    Assert.AreEqual(0, coll.Count());
                }

                using (var coll = new FileBackedAppendOnlyCollection<Type>(fileName, FileMode.Open, typeIO))
                {
                    Assert.AreEqual(false, coll.MetaData);
                    Assert.AreEqual(0, coll.Count);
                    Assert.AreEqual(0, coll.Count());
                }

                File.Delete(fileName);

                using (var coll = new FileBackedAppendOnlyCollection<Type, string>(fileName, FileMode.CreateNew, "Test", typeIO, new StringIO()))
                {
                    coll.AddRange(types);
                    Assert.AreEqual("Test", coll.MetaData);
                    Assert.AreEqual(types.Length, coll.Count);
                    Assert.AreEqual(types.Length, coll.Count());
                    int i = 0;

                    foreach (Type type in coll)
                    {
                        Assert.AreEqual(types[i++], type);
                    }
                }

                using (var coll = new FileBackedAppendOnlyCollection<Type, string>(fileName, FileMode.Open, "Ignored", typeIO, new StringIO()))
                {
                    Assert.AreEqual("Test", coll.MetaData);
                    Assert.AreEqual(types.Length, coll.Count);
                    Assert.AreEqual(types.Length, coll.Count());
                    Assert.AreEqual(types.Length, coll.Count);
                    int i = 0;

                    foreach (Type type in coll)
                    {
                        Assert.AreEqual(types[i++], type);
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
