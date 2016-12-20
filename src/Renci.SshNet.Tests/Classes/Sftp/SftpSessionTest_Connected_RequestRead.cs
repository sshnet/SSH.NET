using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpSessionTest_Connected_RequestRead
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private SftpSession _sftpSession;
        private TimeSpan _operationTimeout;
        private byte[] _actual;
        private byte[] _expected;
        private Encoding _encoding;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        protected void Arrange()
        {
            var random = new Random();

            _operationTimeout = TimeSpan.FromMilliseconds(random.Next(100, 500));
            _expected = new byte[random.Next(30, 50)];
            _encoding = Encoding.UTF8;
            random.NextBytes(_expected);

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);

            var sequence = new MockSequence();

            _sessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Open());
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendSubsystemRequest("sftp")).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(It.IsAny<byte[]>())).Callback(
                () =>
                    {
                        // generate response for SftpInitRequest
                        var versionInfoResponse = SftpVersionResponseBuilder.Create(3)
                                                                            .Build();
                        _channelSessionMock.Raise(
                            c => c.DataReceived += null,
                            new ChannelDataEventArgs(0, versionInfoResponse));
                    });
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(It.IsAny<byte[]>())).Callback(
                () =>
                    {
                        var sftpNameResponse = CreateSftpNameResponse(1, _encoding, "ABC");

                        _channelSessionMock.Raise(
                            c => c.DataReceived += null,
                            new ChannelDataEventArgs(0, sftpNameResponse));
                    }
                );
            _channelSessionMock.InSequence(sequence).Setup(p => p.IsOpen).Returns(true);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(It.IsAny<byte[]>())).Callback(
                () =>
                    {
                        var sftpDataResponse = CreateSftpDataResponse(2, _expected);

                        _channelSessionMock.Raise(
                            c => c.DataReceived += null,
                            new ChannelDataEventArgs(0, sftpDataResponse.Take(0, 20)));
                        _channelSessionMock.Raise(
                            c => c.DataReceived += null,
                            new ChannelDataEventArgs(0, sftpDataResponse.Take(20, sftpDataResponse.Length - 20)));
                    }
                );

            _sftpSession = new SftpSession(_sessionMock.Object, _operationTimeout, _encoding);
            _sftpSession.Connect();
        }

        protected void Act()
        {
            _actual = _sftpSession.RequestRead(new byte[0], 0, 200);
        }

        [TestMethod]
        public void ReturnedValueShouldBeDataOfSftpDataResponse()
        {
            Assert.IsNotNull(_actual);
            Assert.IsTrue(_expected.SequenceEqual(_actual));
        }

        private static byte[] CreateSftpDataResponse(uint responseId, byte[] data)
        {
            var sshDataStream = new SshDataStream(4 + 1 + 4 + 4 + data.Length + 1);
            sshDataStream.Write((uint) sshDataStream.Capacity - 4);
            sshDataStream.WriteByte((byte) SftpMessageTypes.Data);
            sshDataStream.Write(responseId);
            sshDataStream.Write((uint) data.Length);
            sshDataStream.Write(data, 0, data.Length);
            sshDataStream.WriteByte(1); // EOF
            return sshDataStream.ToArray();
        }

        private static byte[] CreateSftpNameResponse(uint responseId, Encoding encoding, params string[] names)
        {
            var namesAndAttributes = new List<byte>();
            foreach (var name in names)
            {
                var nameBytes = encoding.GetBytes(name);
                var attributesBytes = SftpFileAttributes.Empty.GetBytes();

                namesAndAttributes.AddRange((((uint) nameBytes.Length).GetBytes())); // filename length
                namesAndAttributes.AddRange(nameBytes); // filename
                namesAndAttributes.AddRange(((uint) 0).GetBytes()); // longname length
                namesAndAttributes.AddRange(attributesBytes); // attributes
            }

            var namesAndAttributesBytes = namesAndAttributes.ToArray();

            var sshDataStream = new SshDataStream(4 + 1 + 4 + 4 + namesAndAttributesBytes.Length);
            sshDataStream.Write((uint) sshDataStream.Capacity - 4);
            sshDataStream.WriteByte((byte) SftpMessageTypes.Name);
            sshDataStream.Write(responseId);
            sshDataStream.Write((uint) names.Length);
            sshDataStream.Write(namesAndAttributesBytes, 0, namesAndAttributesBytes.Length);
            return sshDataStream.ToArray();
        }
    }
}