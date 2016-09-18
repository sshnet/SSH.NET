using System.Globalization;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    [TestClass]
    public class ChannelOpenMessageTest
    {
        private Random _random;
        private Encoding _ascii;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            _ascii = Encoding.ASCII;
        }

        [TestMethod]
        public void DefaultConstructor()
        {
            var target = new ChannelOpenMessage();

            Assert.IsNull(target.ChannelType);
            Assert.IsNull(target.Info);
            Assert.AreEqual(default(uint), target.InitialWindowSize);
            Assert.AreEqual(default(uint), target.LocalChannelNumber);
            Assert.AreEqual(default(uint), target.MaximumPacketSize);
        }

        [TestMethod]
        public void Constructor_LocalChannelNumberAndInitialWindowSizeAndMaximumPacketSizeAndInfo()
        {
            var localChannelNumber = (uint) _random.Next(0, int.MaxValue);
            var initialWindowSize = (uint) _random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint) _random.Next(0, int.MaxValue);
            var info = new DirectTcpipChannelInfo("host", 22, "originator", 25);

            var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);

            Assert.AreEqual(info.ChannelType, _ascii.GetString(target.ChannelType));
            Assert.AreSame(info, target.Info);
            Assert.AreEqual(initialWindowSize, target.InitialWindowSize);
            Assert.AreEqual(localChannelNumber, target.LocalChannelNumber);
            Assert.AreEqual(maximumPacketSize, target.MaximumPacketSize);
        }

        [TestMethod]
        public void Constructor_LocalChannelNumberAndInitialWindowSizeAndMaximumPacketSizeAndInfo_ShouldThrowArgumentNullExceptionWhenInfoIsNull()
        {
            var localChannelNumber = (uint) _random.Next(0, int.MaxValue);
            var initialWindowSize = (uint) _random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint) _random.Next(0, int.MaxValue);
            ChannelOpenInfo info = null;

            try
            {
                new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("info", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetBytes()
        {
            var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
            var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
            var info = new DirectTcpipChannelInfo("host", 22, "originator", 25);
            var infoBytes = info.GetBytes();
            var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);

            var bytes = target.GetBytes();

            var expectedBytesLength = 1; // Type
            expectedBytesLength += 4; // ChannelType length
            expectedBytesLength += target.ChannelType.Length; // ChannelType
            expectedBytesLength += 4; // LocalChannelNumber
            expectedBytesLength += 4; // InitialWindowSize
            expectedBytesLength += 4; // MaximumPacketSize
            expectedBytesLength += infoBytes.Length; // Info

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual(ChannelOpenMessage.MessageNumber, sshDataStream.ReadByte());

            var actualChannelTypeLength = sshDataStream.ReadUInt32();
            Assert.AreEqual((uint) target.ChannelType.Length, actualChannelTypeLength);

            var actualChannelType = new byte[actualChannelTypeLength];
            sshDataStream.Read(actualChannelType, 0, (int) actualChannelTypeLength);
            Assert.IsTrue(target.ChannelType.SequenceEqual(actualChannelType));

            Assert.AreEqual(localChannelNumber, sshDataStream.ReadUInt32());
            Assert.AreEqual(initialWindowSize, sshDataStream.ReadUInt32());
            Assert.AreEqual(maximumPacketSize, sshDataStream.ReadUInt32());

            var actualInfo = new byte[infoBytes.Length];
            sshDataStream.Read(actualInfo, 0, actualInfo.Length);
            Assert.IsTrue(infoBytes.SequenceEqual(actualInfo));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }

        [TestMethod]
        public void Load_DirectTcpipChannelInfo()
        {
            var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
            var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
            var info = new DirectTcpipChannelInfo("host", 22, "originator", 25);
            var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
            var bytes = target.GetBytes();

            target.Load(bytes, 1, bytes.Length - 1); // skip message type

            Assert.AreEqual(info.ChannelType, _ascii.GetString(target.ChannelType));
            Assert.IsNotNull(target.Info);
            Assert.AreEqual(initialWindowSize, target.InitialWindowSize);
            Assert.AreEqual(localChannelNumber, target.LocalChannelNumber);
            Assert.AreEqual(maximumPacketSize, target.MaximumPacketSize);

            var directTcpChannelInfo = target.Info as DirectTcpipChannelInfo;
            Assert.IsNotNull(directTcpChannelInfo);
            Assert.AreEqual(info.ChannelType, directTcpChannelInfo.ChannelType);
            Assert.AreEqual(info.HostToConnect, directTcpChannelInfo.HostToConnect);
            Assert.AreEqual(info.OriginatorAddress, directTcpChannelInfo.OriginatorAddress);
            Assert.AreEqual(info.OriginatorPort, directTcpChannelInfo.OriginatorPort);
            Assert.AreEqual(info.PortToConnect, directTcpChannelInfo.PortToConnect);
        }

        [TestMethod]
        public void Load_ForwardedTcpipChannelInfo()
        {
            var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
            var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
            var info = new ForwardedTcpipChannelInfo("connected", 25, "originator", 21);
            var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
            var bytes = target.GetBytes();

            target.Load(bytes, 1, bytes.Length - 1); // skip message type

            Assert.AreEqual(info.ChannelType, _ascii.GetString(target.ChannelType));
            Assert.IsNotNull(target.Info);
            Assert.AreEqual(initialWindowSize, target.InitialWindowSize);
            Assert.AreEqual(localChannelNumber, target.LocalChannelNumber);
            Assert.AreEqual(maximumPacketSize, target.MaximumPacketSize);

            var forwardedTcpipChannelInfo = target.Info as ForwardedTcpipChannelInfo;
            Assert.IsNotNull(forwardedTcpipChannelInfo);
            Assert.AreEqual(info.ChannelType, forwardedTcpipChannelInfo.ChannelType);
            Assert.AreEqual(info.ConnectedAddress, forwardedTcpipChannelInfo.ConnectedAddress);
            Assert.AreEqual(info.ConnectedPort, forwardedTcpipChannelInfo.ConnectedPort);
            Assert.AreEqual(info.OriginatorAddress, forwardedTcpipChannelInfo.OriginatorAddress);
            Assert.AreEqual(info.OriginatorPort, forwardedTcpipChannelInfo.OriginatorPort);
        }

        [TestMethod]
        public void Load_SessionChannelOpenInfo()
        {
            var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
            var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
            var info = new SessionChannelOpenInfo();
            var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
            var bytes = target.GetBytes();

            target.Load(bytes, 1, bytes.Length - 1); // skip message type

            Assert.AreEqual(info.ChannelType, _ascii.GetString(target.ChannelType));
            Assert.IsNotNull(target.Info);
            Assert.AreEqual(initialWindowSize, target.InitialWindowSize);
            Assert.AreEqual(localChannelNumber, target.LocalChannelNumber);
            Assert.AreEqual(maximumPacketSize, target.MaximumPacketSize);

            var sessionChannelOpenInfo = target.Info as SessionChannelOpenInfo;
            Assert.IsNotNull(sessionChannelOpenInfo);
            Assert.AreEqual(info.ChannelType, sessionChannelOpenInfo.ChannelType);
        }


        [TestMethod]
        public void Load_X11ChannelOpenInfo()
        {
            var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
            var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
            var info = new X11ChannelOpenInfo("address", 26);
            var target = new ChannelOpenMessage(localChannelNumber, initialWindowSize, maximumPacketSize, info);
            var bytes = target.GetBytes();

            target.Load(bytes, 1, bytes.Length - 1); // skip message type

            Assert.AreEqual(info.ChannelType, _ascii.GetString(target.ChannelType));
            Assert.IsNotNull(target.Info);
            Assert.AreEqual(initialWindowSize, target.InitialWindowSize);
            Assert.AreEqual(localChannelNumber, target.LocalChannelNumber);
            Assert.AreEqual(maximumPacketSize, target.MaximumPacketSize);

            var x11ChannelOpenInfo = target.Info as X11ChannelOpenInfo;
            Assert.IsNotNull(x11ChannelOpenInfo);
            Assert.AreEqual(info.ChannelType, x11ChannelOpenInfo.ChannelType);
            Assert.AreEqual(info.OriginatorAddress, x11ChannelOpenInfo.OriginatorAddress);
            Assert.AreEqual(info.OriginatorPort, x11ChannelOpenInfo.OriginatorPort);
        }

        [TestMethod]
        public void Load_ShouldThrowNotSupportedExceptionWhenChannelTypeIsNotSupported()
        {
            var localChannelNumber = (uint)_random.Next(0, int.MaxValue);
            var initialWindowSize = (uint)_random.Next(0, int.MaxValue);
            var maximumPacketSize = (uint)_random.Next(0, int.MaxValue);
            var channelName = "dunno_" + _random.Next().ToString(CultureInfo.InvariantCulture);
            var channelType = _ascii.GetBytes(channelName);

            var sshDataStream = new SshDataStream(1 + 4 + channelType.Length + 4 + 4 + 4);
            sshDataStream.WriteByte(ChannelOpenMessage.MessageNumber);
            sshDataStream.Write((uint) channelType.Length);
            sshDataStream.Write(channelType, 0, channelType.Length);
            sshDataStream.Write(localChannelNumber);
            sshDataStream.Write(initialWindowSize);
            sshDataStream.Write(maximumPacketSize);
            var bytes = sshDataStream.ToArray();
            var target = new ChannelOpenMessage();

            try
            {
                target.Load(bytes, 1, bytes.Length - 1); // skip message type
                Assert.Fail();
            }
            catch (NotSupportedException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("Channel type '{0}' is not supported.", channelName), ex.Message);
            }
        }
    }
}
