using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sayer.Strings.Test
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void TestParseInt()
        {
            string text = "7";
            int value = text.Parse<int>(defaultForNullOrEmpty: false);
            Assert.AreEqual(7, value);
            text = "8";
            Assert.IsTrue(text.TryParse(result: out value));
            Assert.AreEqual(8, value);
            value = 7;
            Assert.IsTrue(text.TryParse(defaultForNullOrEmpty: false, result: out value));
            Assert.AreEqual(8, value);
            text = "foo";

            try
            {
                text.Parse<int>();
                Assert.Fail("Did not expect to get here");
            }
            catch (Exception)
            {
                // expected
            }

            try
            {
                text.Parse<int>(defaultForNullOrEmpty: false);
                Assert.Fail("Did not expect to get here");
            }
            catch (Exception)
            {
                // expected
            }

            Assert.IsFalse(text.TryParse(result: out value));
            Assert.AreEqual(0, value);
            value = 1;
            Assert.IsFalse(text.TryParse(defaultForNullOrEmpty: false, result: out value));
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void TestParseNull()
        {
            int value = StringExtensions.Parse<int>(text: null, defaultForNullOrEmpty: true);
            Assert.AreEqual(0, value);
            value = 1;
            Assert.IsTrue(StringExtensions.TryParse(text: null, defaultForNullOrEmpty: true, result: out value));
            Assert.AreEqual(0, value);

            try
            {
                StringExtensions.Parse<int>(text: null);
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
                // expected
            }

            try
            {
                StringExtensions.Parse<int>(text: null, defaultForNullOrEmpty: false);
                Assert.Fail("Did not expect to get here");
            }
            catch (NotSupportedException)
            {
                // expected
            }

            value = 1;
            Assert.IsFalse(StringExtensions.TryParse(text: null, result: out value));
            Assert.AreEqual(0, value);
            value = 1;
            Assert.IsFalse(StringExtensions.TryParse(text: null, defaultForNullOrEmpty: false, result: out value));
            Assert.AreEqual(0, value);
        }

        private enum PeriodLength { Daily, Weekly }

        [TestMethod]
        public void TestParseEnum()
        {
            string text = "Weekly";
            PeriodLength value = text.Parse<PeriodLength>(defaultForNullOrEmpty: false);
            Assert.AreEqual(PeriodLength.Weekly, value);
            text = "Daily";
            Assert.IsTrue(text.TryParse(defaultForNullOrEmpty: false, result: out value));
            Assert.AreEqual(PeriodLength.Daily, value);
            text = "foo";

            try
            {
                text.Parse<int>(defaultForNullOrEmpty: false);
                Assert.Fail("Did not expect to get here");
            }
            catch (Exception)
            {
                // expected
            }

            Assert.IsFalse(text.TryParse(defaultForNullOrEmpty: false, result: out value));
            Assert.AreEqual((PeriodLength)0, value);
        }

        [TestMethod]
        public void TestParseNullable()
        {
            string text = "";
            PeriodLength? value = text.Parse<PeriodLength?>(defaultForNullOrEmpty: true);
            Assert.IsNull(value);
            Assert.IsTrue(text.TryParse(defaultForNullOrEmpty: true, result: out value));
            Assert.IsNull(value);
            text = "Weekly";
            value = text.Parse<PeriodLength?>(defaultForNullOrEmpty: true);
            Assert.AreEqual(PeriodLength.Weekly, value);
            text = "Daily";
            Assert.IsTrue(text.TryParse(defaultForNullOrEmpty: true, result: out value));
            Assert.AreEqual(PeriodLength.Daily, value);
        }
    }
}
