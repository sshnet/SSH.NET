using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpSessionTest_Connected_RequestRead
    {
        #region SftpSession.Connect()

        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private ISftpResponseFactory _sftpResponseFactory;
        private SftpSession _sftpSession;
        private int _operationTimeout;
        private Encoding _encoding;
        private uint _protocolVersion;
        private byte[] _sftpInitRequestBytes;
        private SftpVersionResponse _sftpVersionResponse;
        private byte[] _sftpRealPathRequestBytes;
        private SftpNameResponse _sftpNameResponse;

        #endregion SftpSession.Connect()

        private byte[] _sftpReadRequestBytes;
        private byte[] _sftpDataResponseBytes;
        private byte[] _handle;
        private uint _offset;
        private uint _length;
        private byte[] _data;
        private byte[] _actual;

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
            _protocolVersion = (uint) random.Next(0, 3);
            _encoding = Encoding.UTF8;
            _sftpResponseFactory = new SftpResponseFactory();
            _sftpInitRequestBytes = new SftpInitRequestBuilder().WithVersion(SftpSession.MaximumSupportedVersion)
                                                                .Build()
                                                                .GetBytes();
            _sftpVersionResponse = new SftpVersionResponseBuilder().WithVersion(_protocolVersion)
                                                                   .Build();
            _sftpRealPathRequestBytes = new SftpRealPathRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                        .WithRequestId(1)
                                                                        .WithPath(".")
                                                                        .WithEncoding(_encoding)
                                                                        .Build()
                                                                        .GetBytes();
            _sftpNameResponse = new SftpNameResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                             .WithResponseId(1)
                                                             .WithEncoding(_encoding)
                                                             .WithFile("XYZ", SftpFileAttributes.Empty)
                                                             .Build();

            #endregion SftpSession.Connect()

            _handle = CryptoAbstraction.GenerateRandom(random.Next(1, 10));
            _offset = (uint) random.Next(1, 5);
            _length = (uint) random.Next(30, 50);
            _data = CryptoAbstraction.GenerateRandom((int) _length);
            _sftpReadRequestBytes = new SftpReadRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                .WithRequestId(2)
                                                                .WithHandle(_handle)
                                                                .WithOffset(_offset)
                                                                .WithLength(_length)
                                                                .Build()
                                                                .GetBytes();
            _sftpDataResponseBytes = new SftpDataResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                                  .WithResponseId(2)
                                                                  .WithData(_data)
                                                                  .Build()
                                                                  .GetBytes();
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
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpInitRequestBytes))
                                                    .Callback(() =>
                                                    {
                                                        _channelSessionMock.Raise(c => c.DataReceived += null,
                                                                                  new ChannelDataEventArgs(0, _sftpVersionResponse.GetBytes()));
                                                    });
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpRealPathRequestBytes))
                                                    .Callback(() =>
                                                    {
                                                        _channelSessionMock.Raise(c => c.DataReceived += null,
                                                                                  new ChannelDataEventArgs(0, _sftpNameResponse.GetBytes()));
                                                    });

            #endregion SftpSession.Connect()

            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpReadRequestBytes))
                                                    .Callback(() =>
                    {
                        _channelSessionMock.Raise(
                            c => c.DataReceived += null,
                            new ChannelDataEventArgs(0, _sftpDataResponseBytes.Take(0, 20)));
                        _channelSessionMock.Raise(
                            c => c.DataReceived += null,
                            new ChannelDataEventArgs(0, _sftpDataResponseBytes.Take(20, _sftpDataResponseBytes.Length - 20)));
                    }
                );
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _sftpSession = new SftpSession(_sessionMock.Object, _operationTimeout, _encoding, _sftpResponseFactory);
            _sftpSession.Connect();
        }

        protected void Act()
        {
            _actual = _sftpSession.RequestRead(_handle, _offset, _length);
        }

        [TestMethod]
        public void ReturnedValueShouldBeDataOfSftpDataResponse()
        {
            Assert.IsNotNull(_actual);
            Assert.IsTrue(_data.SequenceEqual(_actual));
        }
    }
}