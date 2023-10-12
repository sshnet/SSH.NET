 using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp.Responses
{
    [TestClass]
    public class SftpHandleResponseTest
    {
        private Random _random;
        private uint _protocolVersion;
        private uint _responseId;
        private byte[] _handle;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            _protocolVersion = (uint) _random.Next(0, int.MaxValue);
            _responseId = (uint) _random.Next(0, int.MaxValue);
            _handle = new byte[_random.Next(1, 10)];
            _random.NextBytes(_handle);
        }

        [TestMethod]
        public void Constructor()
        {
            var target = new SftpHandleResponse(_protocolVersion);

            Assert.IsNull(target.Handle);
            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual((uint) 0, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.Handle, target.SftpMessageType);
        }

        [TestMethod]
        public void Load()
        {
            var target = new SftpHandleResponse(_protocolVersion);

            var sshDataStream = new SshDataStream(4 + _handle.Length);
            sshDataStream.Write(_responseId);
            sshDataStream.Write((uint) _handle.Length);
            sshDataStream.Write(_handle, 0, _handle.Length);

            target.Load(sshDataStream.ToArray());

            Assert.IsNotNull(target.Handle);
            Assert.IsTrue(target.Handle.SequenceEqual(_handle));
            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual(_responseId, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.Handle, target.SftpMessageType);
        }
    }
}
