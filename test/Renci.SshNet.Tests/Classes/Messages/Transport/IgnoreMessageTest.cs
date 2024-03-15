using System;
using System.Linq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    [TestClass]
    internal class IgnoreMessageTest
    {
        private Random _random;
        private byte[] _data;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            _data = new byte[_random.Next(1, 10)];
            _random.NextBytes(_data);
        }

        [TestMethod]
        public void DefaultConstructor()
        {
            var target = new IgnoreMessage();
            Assert.IsNotNull(target.Data);
            Assert.AreEqual(0, target.Data.Length);
        }

        [TestMethod]
        public void Constructor_Data()
        {
            var target = new IgnoreMessage(_data);
            Assert.AreSame(_data, target.Data);
        }

        [TestMethod]
        public void Constructor_Data_ShouldThrowArgumentNullExceptionWhenDataIsNull()
        {
            const byte[] data = null;

            try
            {
                new IgnoreMessage(data);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("data", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new IgnoreMessage(_data);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // Data length
            expectedBytesLength += _data.Length; // Data

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual(request.MessageNumber, sshDataStream.ReadByte());
            Assert.AreEqual((uint) _data.Length, sshDataStream.ReadUInt32());

            var actualData = new byte[_data.Length];
            sshDataStream.Read(actualData, 0, actualData.Length);
            Assert.IsTrue(_data.SequenceEqual(actualData));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }

        [TestMethod]
        public void Load_IgnoresData()
        {
            var ignoreMessage = new IgnoreMessage(_data);
            var bytes = ignoreMessage.GetBytes();
            var target = new IgnoreMessage();

            target.Load(bytes, 1, bytes.Length - 1);

            Assert.IsNotNull(target.Data);
            Assert.AreEqual(0, target.Data.Length);
        }
    }
}
