using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;

namespace Sayer.Collections.Test
{
    [TestClass]
    public class NameValueCollectionExtensionsTest
    {
        [TestMethod]
        public void DefaultValueTest()
        {
            var nameValueCollection = new NameValueCollection();
            Assert.AreEqual(1.7, nameValueCollection.Get("foo", 1.7));
            Assert.IsNull(nameValueCollection.Get("foo"));
        }

        [TestMethod]
        public void ExistingValueTest()
        {
            var nameValueCollection = new NameValueCollection { { "foo", 1.7 } };
            Assert.AreEqual(1.7, nameValueCollection.Get("foo", 2.8));
            Assert.AreEqual("1.7", nameValueCollection.Get("foo"));
        }

        [TestMethod]
        public void NullTest()
        {
            var nameValueCollection = new NameValueCollection();
            Assert.IsNull(nameValueCollection.Get<object>("foo", null));
            nameValueCollection.Add<object>("foo", null);
            Assert.IsNull(nameValueCollection.Get<object>("foo", null));
            Assert.IsInstanceOfType(nameValueCollection.Get("foo", new object()), typeof(object));
        }

        [TestMethod]
        public void TryGetTest()
        {
            var nameValueCollection = new NameValueCollection();
            Assert.IsFalse(nameValueCollection.TryGet("foo", out double value));
            Assert.AreEqual(0.0, value);
            nameValueCollection.Add("foo", 1.7);
            Assert.IsTrue(nameValueCollection.TryGet("foo", out double value2));
            Assert.AreEqual(1.7, value2);
        }

        [TestMethod]
        public void GetShouldFailWhenMultipleStoredValuesTest()
        {
            var nameValueCollection = new NameValueCollection { { "foo", "1.7" }, { "foo", "1.8" } };
            string[] values = nameValueCollection.GetValues("foo");
            Assert.IsNotNull(values);
            Assert.AreEqual(2, values.Length);
            Assert.AreEqual("1.7,1.8", nameValueCollection.Get("foo"));

            try
            {
                nameValueCollection.TryGet("foo", out double value);
                Assert.Fail("Did not expect to get here");
            }
            catch (Exception)
            {
                // expected
            }
        }
    }
}
