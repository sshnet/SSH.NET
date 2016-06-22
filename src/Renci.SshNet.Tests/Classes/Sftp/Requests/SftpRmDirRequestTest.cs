using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp.Requests
{
    [TestClass]
    public class SftpRmDirRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private Encoding _encoding;
        private string _path;
        private byte[] _pathBytes;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _encoding = Encoding.Unicode;
            _path = random.Next().ToString(CultureInfo.InvariantCulture);
            _pathBytes = _encoding.GetBytes(_path);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpRmDirRequest(_protocolVersion, _requestId, _path, _encoding, null);

            Assert.AreSame(_encoding, request.Encoding);
            Assert.AreEqual(_path, request.Path);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.RmDir, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();
            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpRmDirRequest(_protocolVersion, _requestId, _path, _encoding, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();
            var nameActionInvocations = new List<SftpNameResponse>();
            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var request = new SftpRmDirRequest(_protocolVersion, _requestId, _path, _encoding, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Path length
            expectedBytesLength += _pathBytes.Length; // Path

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.RmDir, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _pathBytes.Length, sshDataStream.ReadUInt32());
            var actualPath = new byte[_pathBytes.Length];
            sshDataStream.Read(actualPath, 0, actualPath.Length);
            Assert.IsTrue(_pathBytes.SequenceEqual(actualPath));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}