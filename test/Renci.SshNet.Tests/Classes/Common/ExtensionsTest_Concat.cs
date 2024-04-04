using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ExtensionsTest_Concat
    {
        private Random _random;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
        }

        [TestMethod]
        public void ShouldReturnSecondWhenFirstIsEmpty()
        {
            var first = Array.Empty<byte>();
            var second = CreateBuffer(16);

            var actual = Extensions.Concat(first, second);

            Assert.IsNotNull(actual);
            Assert.AreEqual(second, actual);
        }

        [TestMethod]
        public void ShouldReturnSecondWhenFirstIsNull()
        {
            const byte[] first = null;
            var second = CreateBuffer(16);

            var actual = Extensions.Concat(first, second);

            Assert.IsNotNull(actual);
            Assert.AreEqual(second, actual);
        }

        [TestMethod]
        public void ShouldReturnFirstWhenSecondIsEmpty()
        {
            var first = CreateBuffer(16);
            var second = Array.Empty<byte>();

            var actual = Extensions.Concat(first, second);

            Assert.IsNotNull(actual);
            Assert.AreEqual(first, actual);
        }

        [TestMethod]
        public void ShouldReturnFirstWhenSecondIsNull()
        {
            var first = CreateBuffer(16);
            const byte[] second = null;

            var actual = Extensions.Concat(first, second);

            Assert.IsNotNull(actual);
            Assert.AreEqual(first, actual);
        }

        [TestMethod]
        public void ShouldReturnNullWhenFirstAndSecondAreNull()
        {
            const byte[] first = null;
            const byte[] second = null;

            var actual = Extensions.Concat(first, second);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ShouldConcatSecondToFirstWhenBothAreNotEmpty()
        {
            var first = CreateBuffer(4);
            var second = CreateBuffer(2);

            var actual = Extensions.Concat(first, second);

            Assert.IsNotNull(actual);
            Assert.AreEqual(first.Length + second.Length, actual.Length);
            Assert.AreEqual(first[0], actual[0]);
            Assert.AreEqual(first[1], actual[1]);
            Assert.AreEqual(first[2], actual[2]);
            Assert.AreEqual(first[3], actual[3]);
            Assert.AreEqual(second[0], actual[4]);
            Assert.AreEqual(second[1], actual[5]);
        }

        private byte[] CreateBuffer(int length)
        {
            var buffer = new byte[length];
            _random.NextBytes(buffer);
            return buffer;
        }
    }
}
