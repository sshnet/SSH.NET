using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class ExtensionsTest_ToBigInteger2
    {
        [TestMethod]
        public void ShouldNotAppendZero()
        {
            byte[] value = { 0x0a, 0x0d };

            var actual = value.ToBigInteger2().ToByteArray().Reverse();

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(0x0a, actual[0]);
            Assert.AreEqual(0x0d, actual[1]);
        }

        [TestMethod]
        public void ShouldAppendZero()
        {
            byte[] value = { 0xff, 0x0a, 0x0d };

            var actual = value.ToBigInteger2().ToByteArray().Reverse();

            Assert.IsNotNull(actual);
            Assert.AreEqual(4, actual.Length);
            Assert.AreEqual(0x00, actual[0]);
            Assert.AreEqual(0xff, actual[1]);
            Assert.AreEqual(0x0a, actual[2]);
            Assert.AreEqual(0x0d, actual[3]);
        }
    }
}
