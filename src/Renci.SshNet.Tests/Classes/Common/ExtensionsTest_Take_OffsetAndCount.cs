using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class ExtensionsTest_Take_OffsetAndCount
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            const byte[] value = null;
            const int offset = 0;
            const int count = 0;

            try
            {
                Extensions.Take(value, offset, count);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldReturnEmptyByteArrayWhenCountIsZero()
        {
            var value = CreateBuffer(16);
            const int offset = 25;
            const int count = 0;


            var actual = Extensions.Take(value, offset, count);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void ShouldReturnValueWhenCountIsEqualToLengthOfValueAndOffsetIsZero()
        {
            var value = CreateBuffer(16);
            const int offset = 0;
            var count = value.Length;

            var actual = Extensions.Take(value, offset, count);

            Assert.IsNotNull(actual);
            Assert.AreEqual(value.Length, actual.Length);
            Assert.AreEqual(value, actual);
        }

        [TestMethod]
        public void ShouldReturnLeadingBytesWhenOffsetIsZeroAndCountIsLessThanLengthOfValue()
        {
            var value = CreateBuffer(16);
            const int offset = 0;
            const int count = 5;

            var actual = Extensions.Take(value, offset, count);

            Assert.IsNotNull(actual);
            Assert.AreEqual(count, actual.Length);
            Assert.AreEqual(value[0], actual[0]);
            Assert.AreEqual(value[1], actual[1]);
            Assert.AreEqual(value[2], actual[2]);
            Assert.AreEqual(value[3], actual[3]);
            Assert.AreEqual(value[4], actual[4]);
        }

        [TestMethod]
        public void ShouldReturnCorrectPartOfValueWhenOffsetIsGreaterThanZeroAndOffsetPlusCountIsLessThanLengthOfValue()
        {
            var value = CreateBuffer(16);
            const int offset = 3;
            const int count = 4;

            var actual = Extensions.Take(value, offset, count);

            Assert.IsNotNull(actual);
            Assert.AreEqual(count, actual.Length);
            Assert.AreEqual(value[3], actual[0]);
            Assert.AreEqual(value[4], actual[1]);
            Assert.AreEqual(value[5], actual[2]);
            Assert.AreEqual(value[6], actual[3]);
        }

        [TestMethod]
        public void ShouldThrowArgumentExceptionWhenCountIsGreaterThanLengthOfValue()
        {
            var value = CreateBuffer(16);
            const int offset = 0;
            var count = value.Length + 1;

            try
            {
                Extensions.Take(value, offset, count);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void ShouldThrowArgumentExceptionWhenOffsetPlusCountIsGreaterThanLengthOfValue()
        {
            var value = CreateBuffer(16);
            const int offset = 1;
            var count = value.Length;

            try
            {
                Extensions.Take(value, offset, count);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        private byte[] CreateBuffer(int length)
        {
            var buffer = new byte[length];
            _random.NextBytes(buffer);
            return buffer;
        }
    }
}
