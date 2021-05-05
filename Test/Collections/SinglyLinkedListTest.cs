using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class SinglyLinkedListTest
    {
        [TestMethod]
        public void TestSinglyLinkedList()
        {
            var list = new SinglyLinkedList<int>();
            Assert.AreEqual(0, list.Count);
            Assert.IsFalse(list.IsReadOnly);
            Assert.IsFalse(list.Contains(0));
            Assert.IsFalse(list.Remove(0));
            list.Clear();

            foreach (int _ in list)
            {
                Assert.Fail("Did not expect to get here");
            }

            for (int i = 1; i <= 10; ++i)
            {
                list.Add(i);
                Assert.AreEqual(i, list.Count);
                Assert.IsTrue(list.Contains(i));
                Assert.IsTrue(list.Remove(i));
                list.Add(i);
            }

            foreach (int i in list)
            {
                Assert.IsTrue(list.Contains(i));
            }

            for (int i = 1; i <= 10; ++i)
            {
                Assert.AreEqual(i, list.PopFirst());
            }

            Assert.AreEqual(0, list.Count);

            for (int i = 1; i <= 10; ++i)
            {
                list.AddLast(i);
                Assert.AreEqual(i, list.Count);
                Assert.IsTrue(list.Contains(i));
            }

            Assert.AreEqual(1, list.First);
            Assert.AreEqual(10, list.Last);
            Assert.IsTrue(list.Remove(5));
            Assert.IsFalse(list.Contains(5));
            Assert.IsFalse(list.Remove(5));
            Assert.AreEqual(9, list.Count);
            Assert.IsTrue(list.Remove(1));
            Assert.AreEqual(8, list.Count);

            while (list.Count > 0)
            {
                list.RemoveFirst();
            }

            for (int i = 1; i <= 10; ++i)
            {
                list.AddFirst(i);
                Assert.AreEqual(i, list.Count);
                Assert.IsTrue(list.Contains(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                Assert.AreEqual(10 - i, list.PopFirst());
            }

            Assert.AreEqual(0, list.Count);

            for (int i = 1; i <= 10; ++i)
            {
                list.AddLast(i);
            }

            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                list.CopyTo(null, 0);
                Assert.Fail("Did not expect to get here.");
            }
            catch (ArgumentNullException)
            {
            }

            int[] array = new int[11];

            try
            {
                list.CopyTo(array, -1);
                Assert.Fail("Did not expect to get here.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                list.CopyTo(array, 2);
                Assert.Fail("Did not expect to get here.");
            }
            catch (ArgumentException)
            {
            }

            list.CopyTo(array, 1);

            foreach (int i in array)
            {
                Assert.AreEqual(i, array[i]);
            }

            list.Clear();
            Assert.AreEqual(0, list.Count);
        }
    }
}
