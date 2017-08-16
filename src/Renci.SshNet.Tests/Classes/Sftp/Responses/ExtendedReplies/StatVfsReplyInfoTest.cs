using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp.Responses
{
    [TestClass]
    public class StatVfsReplyInfoTest
    {
        private Random _random;
        private uint _responseId;
        private ulong _bsize;
        private ulong _frsize;
        private ulong _blocks;
        private ulong _bfree;
        private ulong _bavail;
        private ulong _files;
        private ulong _ffree;
        private ulong _favail;
        private ulong _sid;
        private ulong _namemax;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            _responseId = (uint) _random.Next(0, int.MaxValue);
            _bsize = (ulong) _random.Next(0, int.MaxValue);
            _frsize = (ulong)_random.Next(0, int.MaxValue);
            _blocks = (ulong)_random.Next(0, int.MaxValue);
            _bfree = (ulong)_random.Next(0, int.MaxValue);
            _bavail = (ulong)_random.Next(0, int.MaxValue);
            _files = (ulong)_random.Next(0, int.MaxValue);
            _ffree = (ulong)_random.Next(0, int.MaxValue);
            _favail = (ulong)_random.Next(0, int.MaxValue);
            _sid = (ulong)_random.Next(0, int.MaxValue);
            _namemax = (ulong)_random.Next(0, int.MaxValue);
        }

        [TestMethod]
        public void Constructor()
        {
            var target = new StatVfsReplyInfo();

            Assert.IsNull(target.Information);
        }

        [TestMethod]
        public void Load()
        {
            var sshDataStream = new SshDataStream(4 + 1 + 4 + 88);
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
            sshDataStream.Write((ulong) 0x1);
            sshDataStream.Write(_namemax);

            var extendedReplyResponse = new SftpExtendedReplyResponse(SftpSession.MaximumSupportedVersion);
            extendedReplyResponse.Load(sshDataStream.ToArray());

            Assert.AreEqual(_responseId, extendedReplyResponse.ResponseId);

            var target = extendedReplyResponse.GetReply<StatVfsReplyInfo>();

            Assert.IsNotNull(target.Information);

            var information = target.Information;
            Assert.AreEqual(_bavail, information.AvailableBlocks);
            Assert.AreEqual(_favail, information.AvailableNodes);
            Assert.AreEqual(_frsize, information.BlockSize);
            Assert.AreEqual(_bsize, information.FileSystemBlockSize);
            Assert.AreEqual(_bfree, information.FreeBlocks);
            Assert.AreEqual(_ffree, information.FreeNodes);
            Assert.IsTrue(information.IsReadOnly);
            Assert.AreEqual(_namemax, information.MaxNameLenght);
            Assert.AreEqual(_sid, information.Sid);
            Assert.IsTrue(information.SupportsSetUid);
            Assert.AreEqual(_blocks, information.TotalBlocks);
            Assert.AreEqual(_files, information.TotalNodes);
        }
    }
}