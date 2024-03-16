using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ExtensionsTest_IsEqualTo_ByteArray
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenLeftIsNull()
        {
            const byte[] left = null;
            var right = CreateBuffer(1);

            try
            {
                _ = Extensions.IsEqualTo(left, right);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("left", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenRightIsNull()
        {
            var left = CreateBuffer(1);
            const byte[] right = null;

            try
            {
                _ = Extensions.IsEqualTo(left, right);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("right", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenLeftAndRightAreNull()
        {
            const byte[] left = null;
            const byte[] right = null;

            try
            {
                _ = Extensions.IsEqualTo(left, right);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("left", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldReturnFalseWhenLeftIsNotEqualToRight()
        {
            Assert.IsFalse(Extensions.IsEqualTo(new byte[] {0x0a}, new byte[] {0x0a, 0x0d}));
            Assert.IsFalse(Extensions.IsEqualTo(new byte[] {0x0a, 0x0d}, new byte[] {0x0a}));
            Assert.IsFalse(Extensions.IsEqualTo(new byte[0], new byte[] { 0x0a }));
            Assert.IsFalse(Extensions.IsEqualTo(new byte[] { 0x0a, 0x0d }, new byte[0]));
        }

        [TestMethod]
        public void ShouldReturnTrueWhenLeftIsEqualToRight()
        {
            Assert.IsTrue(Extensions.IsEqualTo(new byte[] { 0x0a, 0x0d }, new byte[] { 0x0a, 0x0d }));
            Assert.IsTrue(Extensions.IsEqualTo(new byte[0], new byte[0]));
        }

        [TestMethod]
        public void ShouldReturnTrueWhenLeftIsSameAsRight()
        {
            var left = new byte[] { 0x0d, 0x0d };

            Assert.IsTrue(Extensions.IsEqualTo(left, left));
        }

        private byte[] CreateBuffer(int length)
        {
            var buffer = new byte[length];
            _random.NextBytes(buffer);
            return buffer;
        }
    }
}
