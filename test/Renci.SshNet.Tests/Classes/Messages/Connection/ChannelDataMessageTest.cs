using System.Linq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    /// This is a test class for ChannelDataMessageTest and is intended
    /// to contain all ChannelDataMessageTest Unit Tests
    /// </summary>
    [TestClass]
    public class ChannelDataMessageTest : TestBase
    {
        [TestMethod]
        public void DefaultConstructor()
        {
            var target = new ChannelDataMessage();

            Assert.IsNull(target.Data);
            Assert.AreEqual(0, target.Offset);
            Assert.AreEqual(0, target.Size);
        }

        [TestMethod]
        public void Constructor_LocalChannelNumberAndData()
        {
            var random = new Random();

            var localChannelNumber = (uint)random.Next(0, int.MaxValue);
            var data = new byte[3];

            var target = new ChannelDataMessage(localChannelNumber, data);

            Assert.AreSame(data, target.Data);
            Assert.AreEqual(0, target.Offset);
            Assert.AreEqual(data.Length, target.Size);
        }

        [TestMethod]
        public void Constructor_LocalChannelNumberAndData_ShouldThrowArgumentNullExceptionWhenDataIsNull()
        {
            var localChannelNumber = (uint) new Random().Next(0, int.MaxValue);
            const byte[] data = null;

            try
            {
                new ChannelDataMessage(localChannelNumber, data);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("data", ex.ParamName);
            }
        }

        [TestMethod]
        public void Constructor_LocalChannelNumberAndDataAndOffsetAndSize()
        {
            var localChannelNumber = (uint) new Random().Next(0, int.MaxValue);
            var data = new byte[4];
            const int offset = 2;
            const int size = 1;

            var target = new ChannelDataMessage(localChannelNumber, data, offset, size);

            Assert.AreSame(data, target.Data);
            Assert.AreEqual(offset, target.Offset);
            Assert.AreEqual(size, target.Size);
        }

        [TestMethod]
        public void Constructor_LocalChannelNumberAndDataAndOffsetAndSize_ShouldThrowArgumentNullExceptionWhenDataIsNull()
        {
            var localChannelNumber = (uint) new Random().Next(0, int.MaxValue);
            const byte[] data = null;
            const int offset = 0;
            const int size = 0;

            try
            {
                new ChannelDataMessage(localChannelNumber, data, offset, size);
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
            var random = new Random();

            var localChannelNumber = (uint) random.Next(0, int.MaxValue);
            var data = CryptoAbstraction.GenerateRandom(random.Next(10, 20));
            var offset = random.Next(0, data.Length - 1);
            var size = random.Next(0, data.Length - offset);

            var target = new ChannelDataMessage(localChannelNumber, data, offset, size);

            var bytes = target.GetBytes();

            var expectedBytesLength = 1; // Type
            expectedBytesLength += 4; // LocalChannelNumber
            expectedBytesLength += 4; // Data length
            expectedBytesLength += size; // Data

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual(ChannelDataMessage.MessageNumber, sshDataStream.ReadByte());
            Assert.AreEqual(localChannelNumber, sshDataStream.ReadUInt32());
            Assert.AreEqual((uint) size, sshDataStream.ReadUInt32());

            var actualData = new byte[size];
            sshDataStream.Read(actualData, 0, size);
            Assert.IsTrue(actualData.SequenceEqual(data.Take(offset, size)));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }

        [TestMethod]
        public void Load()
        {
            var random = new Random();

            var localChannelNumber = (uint) random.Next(0, int.MaxValue);
            var data = CryptoAbstraction.GenerateRandom(random.Next(10, 20));

            var offset = random.Next(0, data.Length - 1);
            var size = random.Next(0, data.Length - offset);
            var channelDataMessage = new ChannelDataMessage(localChannelNumber, data, offset, size);
            var bytes = channelDataMessage.GetBytes();
            var target = new ChannelDataMessage();

            target.Load(bytes, 1, bytes.Length - 1); // skip message type

            Assert.IsTrue(target.Data.SequenceEqual(data.Take(offset, size)));
            Assert.AreEqual(0, target.Offset);
            Assert.AreEqual(size, target.Size);
        }
    }
}
