using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpSessionTest_DataReceived_MultipleSftpMessagesInSingleSshDataMessage
    {
        #region SftpSession.Connect()

        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private Mock<ISftpResponseFactory> _sftpResponseFactoryMock;
        private SftpSession _sftpSession;
        private int _operationTimeout;
        private Encoding _encoding;
        private uint _protocolVersion;
        private byte[] _sftpInitRequestBytes;
        private SftpVersionResponse _sftpVersionResponse;
        private byte[] _sftpRealPathRequestBytes;
        private SftpNameResponse _sftpNameResponse;
        private byte[] _sftpOpenRequestBytes;
        private byte[] _sftpHandleResponseBytes;
        private byte[] _sftpReadRequestBytes;
        private byte[] _sftpDataResponseBytes;
        private byte[] _handle;
        private uint _offset;
        private uint _length;
        private byte[] _data;
        private string _path;
        private byte[] _actualHandle;
        private byte[] _actualData;

        #endregion SftpSession.Connect()

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
            _protocolVersion = (uint)random.Next(0, 3);
            _encoding = Encoding.UTF8;

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
                                                             .WithFile("/ABC", SftpFileAttributes.Empty)
                                                             .Build();

            #endregion SftpSession.Connect()

            _path = random.Next().ToString();
            _handle = CryptoAbstraction.GenerateRandom(4);
            _offset = (uint) random.Next(1, 5);
            _length = (uint) random.Next(30, 50);
            _data = CryptoAbstraction.GenerateRandom(200);
            _sftpOpenRequestBytes = new SftpOpenRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                .WithRequestId(2)
                                                                .WithFileName(_path)
                                                                .WithFlags(Flags.Read)
                                                                .WithEncoding(_encoding)
                                                                .Build()
                                                                .GetBytes();
            _sftpHandleResponseBytes = new SftpHandleResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                                      .WithResponseId(2)
                                                                      .WithHandle(_handle)
                                                                      .Build()
                                                                      .GetBytes();
            _sftpReadRequestBytes = new SftpReadRequestBuilder().WithProtocolVersion(_protocolVersion)
                                                                .WithRequestId(3)
                                                                .WithHandle(_handle)
                                                                .WithOffset(_offset)
                                                                .WithLength(_length)
                                                                .Build()
                                                                .GetBytes();
            _sftpDataResponseBytes = new SftpDataResponseBuilder().WithProtocolVersion(_protocolVersion)
                                                                  .WithResponseId(3)
                                                                  .WithData(_data)
                                                                  .Build()
                                                                  .GetBytes();
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _sftpResponseFactoryMock = new Mock<ISftpResponseFactory>(MockBehavior.Strict);
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
            _sftpResponseFactoryMock.InSequence(sequence)
                                   .Setup(p => p.Create(0U, (byte)SftpMessageTypes.Version, _encoding))
                                   .Returns(_sftpVersionResponse);
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpRealPathRequestBytes))
                                                    .Callback(() =>
                                                    {
                                                        _channelSessionMock.Raise(c => c.DataReceived += null,
                                                                                  new ChannelDataEventArgs(0, _sftpNameResponse.GetBytes()));
                                                    });
            _sftpResponseFactoryMock.InSequence(sequence)
                                   .Setup(p => p.Create(_protocolVersion, (byte)SftpMessageTypes.Name, _encoding))
                                   .Returns(_sftpNameResponse);

            #endregion SftpSession.Connect()

            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpOpenRequestBytes));
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(_sftpReadRequestBytes)).Callback(() =>
                {
                    var sshMessagePayload = new byte[_sftpHandleResponseBytes.Length + _sftpDataResponseBytes.Length];
                    Buffer.BlockCopy(_sftpHandleResponseBytes, 0, sshMessagePayload, 0, _sftpHandleResponseBytes.Length);
                    Buffer.BlockCopy(_sftpDataResponseBytes, 0, sshMessagePayload, _sftpHandleResponseBytes.Length, _sftpDataResponseBytes.Length);

                    _channelSessionMock.Raise(c => c.DataReceived += null,
                                              new ChannelDataEventArgs(0, sshMessagePayload));
                });
            _sftpResponseFactoryMock.InSequence(sequence)
                                   .Setup(p => p.Create(_protocolVersion, (byte) SftpMessageTypes.Handle, _encoding))
                                   .Returns(new SftpHandleResponse(_protocolVersion));
            _sftpResponseFactoryMock.InSequence(sequence)
                                   .Setup(p => p.Create(_protocolVersion, (byte) SftpMessageTypes.Data, _encoding))
                                   .Returns(new SftpDataResponse(_protocolVersion));
        }

        protected void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _sftpSession = new SftpSession(_sessionMock.Object, _operationTimeout, _encoding, _sftpResponseFactoryMock.Object);
            _sftpSession.Connect();
        }

        protected void Act()
        {
            var openAsyncResult = _sftpSession.BeginOpen(_path, Flags.Read, null, null);
            var readAsyncResult = _sftpSession.BeginRead(_handle, _offset, _length, null, null);

            _actualHandle = _sftpSession.EndOpen(openAsyncResult);
            _actualData = _sftpSession.EndRead(readAsyncResult);
        }

        [TestMethod]
        public void ReturnedValueShouldBeDataOfSftpDataResponse()
        {
            Assert.IsTrue(_data.IsEqualTo(_actualData));
        }

        [TestMethod]
        public void ReturnedHandleShouldBeHandleOfSftpHandleResponse()
        {
            Assert.IsTrue(_handle.IsEqualTo(_actualHandle));
        }
    }
}