using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class ExtensionsTest_TrimLeadingZeros
    {
        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            const byte[] value = null;

            try
            {
                Extensions.TrimLeadingZeros(value);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldRemoveAllLeadingZeros()
        {
            byte[] value = {0x00, 0x00, 0x0a, 0x0d};

            var actual = Extensions.TrimLeadingZeros(value);

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(0x0a, actual[0]);
            Assert.AreEqual(0x0d, actual[1]);
        }

        [TestMethod]
        public void ShouldOnlyRemoveLeadingZeros()
        {
            byte[] value = { 0x00, 0x0a, 0x00, 0x0d, 0x00 };

            var actual = Extensions.TrimLeadingZeros(value);

            Assert.IsNotNull(actual);
            Assert.AreEqual(4, actual.Length);
            Assert.AreEqual(0x0a, actual[0]);
            Assert.AreEqual(0x00, actual[1]);
            Assert.AreEqual(0x0d, actual[2]);
            Assert.AreEqual(0x00, actual[3]);
        }

        [TestMethod]
        public void ShouldReturnOriginalEmptyByteArrayWhenValueHasNoLeadingZeros()
        {
            byte[] value = { 0x0a, 0x00, 0x0d };

            var actual = Extensions.TrimLeadingZeros(value);

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Length);
            Assert.AreEqual(0x0a, actual[0]);
            Assert.AreEqual(0x00, actual[1]);
            Assert.AreEqual(0x0d, actual[2]);
        }

        [TestMethod]
        public void ShouldReturnEmptyByteArrayWhenValueIsEmpty()
        {
            byte[] value = {};

            var actual = Extensions.TrimLeadingZeros(value);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length);
        }
    }
}
