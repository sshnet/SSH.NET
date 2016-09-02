using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp.Responses
{
    [TestClass]
    public class SftpExtendedReplyResponseTest
    {
        private Random _random;
        private uint _protocolVersion;
        private uint _responseId;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            _protocolVersion = (uint) _random.Next(0, int.MaxValue);
            _responseId = (uint) _random.Next(0, int.MaxValue);
        }

        [TestMethod]
        public void Constructor()
        {
            var target = new SftpExtendedReplyResponse(_protocolVersion);

            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual((uint) 0, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.ExtendedReply, target.SftpMessageType);
        }

        [TestMethod]
        public void Load()
        {
            var target = new SftpExtendedReplyResponse(_protocolVersion);

            var sshDataStream = new SshDataStream(4);
            sshDataStream.Write(_responseId);

            target.Load(sshDataStream.ToArray());

            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual(_responseId, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.ExtendedReply, target.SftpMessageType);
        }

        [TestMethod]
        public void GetReply_StatVfsReplyInfo()
        {
            var bsize = (ulong) _random.Next(0, int.MaxValue);
            var frsize = (ulong) _random.Next(0, int.MaxValue);
            var blocks = (ulong) _random.Next(0, int.MaxValue);
            var bfree = (ulong) _random.Next(0, int.MaxValue);
            var bavail = (ulong) _random.Next(0, int.MaxValue);
            var files = (ulong) _random.Next(0, int.MaxValue);
            var ffree = (ulong) _random.Next(0, int.MaxValue);
            var favail = (ulong) _random.Next(0, int.MaxValue);
            var sid = (ulong) _random.Next(0, int.MaxValue);
            var namemax = (ulong) _random.Next(0, int.MaxValue);

            var sshDataStream = new SshDataStream(4 + 1 + 4 + 88);
            sshDataStream.Position = 4; // skip 4 bytes for SSH packet length
            sshDataStream.WriteByte((byte)SftpMessageTypes.Attrs);
            sshDataStream.Write(_responseId);
            sshDataStream.Write(bsize);
            sshDataStream.Write(frsize);
            sshDataStream.Write(blocks);
            sshDataStream.Write(bfree);
            sshDataStream.Write(bavail);
            sshDataStream.Write(files);
            sshDataStream.Write(ffree);
            sshDataStream.Write(favail);
            sshDataStream.Write(sid);
            sshDataStream.Write((ulong) 0x2);
            sshDataStream.Write(namemax);

            var sshData = sshDataStream.ToArray();

            var target = new SftpExtendedReplyResponse(_protocolVersion);
            target.Load(sshData, 5, sshData.Length - 5);

            var reply = target.GetReply<StatVfsReplyInfo>();
            Assert.IsNotNull(reply);

            var information = reply.Information;
            Assert.IsNotNull(information);
            Assert.AreEqual(bavail, information.AvailableBlocks);
            Assert.AreEqual(favail, information.AvailableNodes);
            Assert.AreEqual(frsize, information.BlockSize);
            Assert.AreEqual(bsize, information.FileSystemBlockSize);
            Assert.AreEqual(bfree, information.FreeBlocks);
            Assert.AreEqual(ffree, information.FreeNodes);
            Assert.IsFalse(information.IsReadOnly);
            Assert.AreEqual(namemax, information.MaxNameLenght);
            Assert.AreEqual(sid, information.Sid);
            Assert.IsFalse(information.SupportsSetUid);
            Assert.AreEqual(blocks, information.TotalBlocks);
            Assert.AreEqual(files, information.TotalNodes);
        }
    }
}