using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ExtensionsTest_Pad
    {
        [TestMethod]
        public void ShouldReturnNotPadded()
        {
            byte[] value = { 0x0a, 0x0d };
            var padded = value.Pad(2);
#pragma warning disable MSTEST0065 // Avoid Assert.AreEqual on collection types
            Assert.AreEqual(value, padded);
#pragma warning restore MSTEST0065 // Avoid Assert.AreEqual on collection types
            Assert.HasCount(value.Length, padded);
        }

        [TestMethod]
        public void ShouldReturnPadded()
        {
            byte[] value = { 0x0a, 0x0d };
            var padded = value.Pad(3);
            Assert.HasCount(value.Length + 1, padded);
            Assert.AreEqual(0x00, padded[0]);
            Assert.AreEqual(0x0a, padded[1]);
            Assert.AreEqual(0x0d, padded[2]);
        }
    }
}
