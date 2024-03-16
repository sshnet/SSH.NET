using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp.Responses
{
    [TestClass]
    public class SftpAttrsResponseTest
    {
        private Random _random;
        private uint _protocolVersion;
        private uint _responseId;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            _protocolVersion = (uint) _random.Next(0, int.MaxValue);
            _responseId = (uint)_random.Next(0, int.MaxValue);
        }

        [TestMethod]
        public void Constructor()
        {
            var target = new SftpAttrsResponse(_protocolVersion);

            Assert.IsNull(target.Attributes);
            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual((uint) 0, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.Attrs, target.SftpMessageType);
        }

        [TestMethod]
        public void Load()
        {
            var target = new SftpAttrsResponse(_protocolVersion);
            var attributes = CreateSftpFileAttributes();
            var attributesBytes = attributes.GetBytes();

            var sshDataStream = new SshDataStream(4 + attributesBytes.Length);
            sshDataStream.Write(_responseId);
            sshDataStream.Write(attributesBytes, 0, attributesBytes.Length);

            target.Load(sshDataStream.ToArray());

            Assert.IsNotNull(target.Attributes);
            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual(_responseId, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.Attrs, target.SftpMessageType);

            // check attributes in detail
            Assert.AreEqual(attributes.GroupId, target.Attributes.GroupId);
            Assert.AreEqual(attributes.LastWriteTime, target.Attributes.LastWriteTime);
            Assert.AreEqual(attributes.LastWriteTime, target.Attributes.LastWriteTime);
            Assert.AreEqual(attributes.UserId, target.Attributes.UserId);
        }

        private SftpFileAttributes CreateSftpFileAttributes()
        {
            var attributes = SftpFileAttributes.Empty;
            attributes.GroupId = _random.Next();
            attributes.LastAccessTime = new DateTime(2014, 8, 23, 17, 43, 50, DateTimeKind.Local);
            attributes.LastWriteTime = new DateTime(2013, 7, 22, 16, 40, 42, DateTimeKind.Local);
            attributes.UserId = _random.Next();
            return attributes;
        }
    }
}
