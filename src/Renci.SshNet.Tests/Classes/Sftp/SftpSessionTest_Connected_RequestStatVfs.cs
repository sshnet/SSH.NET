using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpSessionTest_Connected_RequestStatVfs
    {
        #region SftpSession.Connect()

        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private ISftpResponseFactory _sftpResponseFactory;
        private SftpSession _sftpSession;
        private int _operationTimeout;
        private Encoding _encoding;
        private uint _protocolVersion;
        private SftpVersionResponse _sftpVersionResponse;
        private SftpNameResponse _sftpNameResponse;
        private byte[] _sftpInitRequestBytes;
        private byte[] _sftpRealPathRequestBytes;

        #endregion SftpSession.Connect()

        private byte[] _sftpStatVfsRequestBytes;
        private StatVfsResponse _sftpStatVfsResponse;
        private ulong _bAvail;
        private string _path;
        private SftpFileSytemInformation _actual;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        private void SetupData()
        {
            var random = new Random();

            #region SftpSession.Connect()

            _operationTimeout = random.Next(100, 500);
            _encoding = Encoding.UTF8;
            _protocolVersion = 3;
            _sftpResponseFactory = new SftpResponseFactory();
            _sftpInitRequestBytes = new SftpInitRequestBuilder().WithVersion(SftpSession.MaximumSupportedVersion)
                                                                .Build()
                                                                .GetBytes();
            _sftpVersionResponse = new SftpVersionResponseBuilder().WithVersion(_protocolVersion)
                                                                   .WithExtension("statvfs@openssh.com", "")
                                                                   .Build();
            _sftpRealPathRequestBytes = new SftpRealPathRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                        .WithRequestId(1)
                                                                        .WithPath(".")
                                                                        .WithEncoding(_encoding)
                                                                        .Build()
                                                                        .GetBytes();
            _sftpNameResponse = new SftpNameResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                             .WithResponseId(1U)
                                                             .WithEncoding(_encoding)
                                                             .WithFile("ABC", SftpFileAttributes.Empty)
                                                             .Build();

            #endregion SftpSession.Connect()

            _path = random.Next().ToString();
            _bAvail = (ulong) random.Next(0, int.MaxValue);
            _sftpStatVfsRequestBytes = new SftpStatVfsRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                      .WithRequestId(2)
                                                                      .WithPath(_path)
                                                                      .WithEncoding(_encoding)
                                                                      .Build()
                                                                      .GetBytes();
            _sftpStatVfsResponse = new SftpStatVfsResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                                   .WithResponseId(2U)
                                                                   .WithBAvail(_bAvail)
                                                                   .Build();
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var sequence = new MockSequence();

            #region SftpSession.Connect()

            _sessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Open());
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendSubsystemRequest("sftp")).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpInitRequestBytes)).Callback(
                () =>
                {
                    _channelSessionMock.Raise(c => c.DataReceived += null,
                                              new ChannelDataEventArgs(0, _sftpVersionResponse.GetBytes()));
                });
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpRealPathRequestBytes)).Callback(
                () =>
                {
                    _channelSessionMock.Raise(c => c.DataReceived += null,
                                              new ChannelDataEventArgs(0, _sftpNameResponse.GetBytes()));
                });

            #endregion SftpSession.Connect()

            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpStatVfsRequestBytes)).Callback(
                () =>
                {
                    _channelSessionMock.Raise(c => c.DataReceived += null,
                                              new ChannelDataEventArgs(0, _sftpStatVfsResponse.GetBytes()));
                });
        }

        protected void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _sftpSession = new SftpSession(_sessionMock.Object, _operationTimeout, _encoding, _sftpResponseFactory);
            _sftpSession.Connect();
        }

        protected void Act()
        {
            _actual = _sftpSession.RequestStatVfs(_path);
        }

        [TestMethod]
        public void ReturnedValueShouldNotBeNull()
        {
            Assert.IsNotNull(_actual);
        }

        [TestMethod]
        public void AvailableBlocksInReturnedValueShouldMatchValueInSftpResponse()
        {
            Assert.AreEqual(_bAvail, _actual.AvailableBlocks);
        }
   }
}