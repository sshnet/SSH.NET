using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpSessionTest_Connected_RequestStatVfs
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private SftpSession _sftpSession;
        private TimeSpan _operationTimeout;
        private SftpFileSytemInformation _actual;
        private Encoding _encoding;

        private ulong _bAvail;

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
            _encoding = Encoding.UTF8;

            _bAvail = (ulong) random.Next(0, int.MaxValue);

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
                                                                            .AddExtension("statvfs@openssh.com", "")
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
                        var statVfsReplyBuilder = StatVfsReplyBuilder.Create(2);
                        statVfsReplyBuilder.WithBAvail(_bAvail);
                        var statVfsReply = statVfsReplyBuilder.Build();

                        _channelSessionMock.Raise(
                            c => c.DataReceived += null,
                            new ChannelDataEventArgs(0, statVfsReply));
                    }
                );

            _sftpSession = new SftpSession(_sessionMock.Object, _operationTimeout, _encoding);
            _sftpSession.Connect();
        }

        protected void Act()
        {
            _actual = _sftpSession.RequestStatVfs("path");
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

        public class StatVfsReplyBuilder
        {
            private readonly uint _responseId;
            private ulong _bsize;
            private ulong _frsize;
            private ulong _blocks;
            private ulong _bfree;
            private ulong _bavail;
            private ulong _files;
            private ulong _ffree;
            private ulong _favail;
            private ulong _sid;
            private ulong _flag;
            private ulong _namemax;

            private StatVfsReplyBuilder(uint responseId)
            {
                _responseId = responseId;
            }

            public static StatVfsReplyBuilder Create(uint responseId)
            {
                return new StatVfsReplyBuilder(responseId);
            }

            public StatVfsReplyBuilder WithBSize(ulong bsize)
            {
                _bsize = bsize;
                return this;
            }

            public StatVfsReplyBuilder WithFrSize(ulong frsize)
            {
                _frsize = frsize;
                return this;
            }

            public StatVfsReplyBuilder WithBlocks(ulong blocks)
            {
                _blocks = blocks;
                return this;
            }

            public StatVfsReplyBuilder WithBFree(ulong bfree)
            {
                _bfree = bfree;
                return this;
            }

            public StatVfsReplyBuilder WithBAvail(ulong bavail)
            {
                _bavail = bavail;
                return this;
            }

            public StatVfsReplyBuilder WithFiles(ulong files)
            {
                _files = files;
                return this;
            }

            public StatVfsReplyBuilder WithFFree(ulong ffree)
            {
                _ffree = ffree;
                return this;
            }

            public StatVfsReplyBuilder WithFAvail(ulong favail)
            {
                _favail = favail;
                return this;
            }

            public StatVfsReplyBuilder WithSid(ulong sid)
            {
                _sid = sid;
                return this;
            }

            public StatVfsReplyBuilder WithIsReadOnly(bool isReadOnly)
            {
                if (isReadOnly)
                    _flag &= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_RDONLY;
                else
                    _flag |= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_RDONLY;

                return this;
            }

            public StatVfsReplyBuilder WithSupportsSetUid(bool supportsSetUid)
            {
                if (supportsSetUid)
                    _flag |= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_NOSUID;
                else
                    _flag &= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_NOSUID;

                return this;
            }

            public StatVfsReplyBuilder WithNameMax(ulong nameMax)
            {
                _namemax = nameMax;
                return this;
            }

            public byte[] Build()
            {
                var sshDataStream = new SshDataStream(4 + 1 + 4 + 88);
                sshDataStream.Write((uint) sshDataStream.Capacity - 4);
                sshDataStream.WriteByte((byte) SftpMessageTypes.ExtendedReply);
                sshDataStream.Write(_responseId);
                sshDataStream.Write(_bsize);
                sshDataStream.Write(_frsize);
                sshDataStream.Write(_blocks);
                sshDataStream.Write(_bfree);
                sshDataStream.Write(_bavail);
                sshDataStream.Write(_files);
                sshDataStream.Write(_ffree);
                sshDataStream.Write(_favail);
                sshDataStream.Write(_sid);
                sshDataStream.Write(_flag);
                sshDataStream.Write(_namemax);
                return sshDataStream.ToArray();
            }
        }
    }
}