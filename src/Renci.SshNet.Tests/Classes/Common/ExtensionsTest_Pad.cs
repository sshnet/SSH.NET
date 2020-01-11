using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class ExtensionsTest_Pad
    {
        [TestMethod]
        public void ShouldReturnNotPadded()
        {
            byte[] value = {0x0a, 0x0d};
            byte[] padded = value.Pad(2);
            Assert.AreEqual(value, padded);
            Assert.AreEqual(value.Length, padded.Length);
        }

        [TestMethod]
        public void ShouldReturnPadded()
        {
            byte[] value = { 0x0a, 0x0d };
            byte[] padded = value.Pad(3);
            Assert.AreEqual(value.Length + 1, padded.Length);
            Assert.AreEqual(0x00, padded[0]);
            Assert.AreEqual(0x0a, padded[1]);
            Assert.AreEqual(0x0d, padded[2]);
        }
    }
}
