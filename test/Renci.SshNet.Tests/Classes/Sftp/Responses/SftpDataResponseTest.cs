using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp.Responses
{
    [TestClass]
    public class SftpDataResponseTest
    {
        private Random _random;
        private uint _protocolVersion;
        private uint _responseId;
        private byte[] _data;

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            _protocolVersion = (uint) _random.Next(0, int.MaxValue);
            _responseId = (uint) _random.Next(0, int.MaxValue);
            _data = new byte[_random.Next(10, 100)];
            _random.NextBytes(_data);
        }

        [TestMethod]
        public void Constructor()
        {
            var target = new SftpDataResponse(_protocolVersion);

            Assert.IsNull(target.Data);
            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual((uint) 0, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.Data, target.SftpMessageType);
        }

        [TestMethod]
        public void Load()
        {
            var target = new SftpDataResponse(_protocolVersion);

            var sshDataStream = new SshDataStream(4 + _data.Length);
            sshDataStream.Write(_responseId);
            sshDataStream.Write((uint) _data.Length);
            sshDataStream.Write(_data, 0, _data.Length);

            var sshData = sshDataStream.ToArray();

            target.Load(sshData);

            Assert.IsNotNull(target.Data);
            Assert.IsTrue(target.Data.SequenceEqual(_data));
            Assert.AreEqual(_protocolVersion, target.ProtocolVersion);
            Assert.AreEqual(_responseId, target.ResponseId);
            Assert.AreEqual(SftpMessageTypes.Data, target.SftpMessageType);
        }
    }
}
