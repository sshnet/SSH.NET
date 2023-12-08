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

namespace Renci.SshNet.Tests.Classes.Sftp.Requests.ExtendedRequests
{
    [TestClass]
    public class StatVfsRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private string _name;
        private string _path;
        private byte[] _pathBytes;
        private byte[] _nameBytes;
        private Encoding _encoding;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _encoding = Encoding.Unicode;
            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _path = random.Next().ToString(CultureInfo.InvariantCulture);
            _pathBytes = _encoding.GetBytes(_path);

            _name = "statvfs@openssh.com";
            _nameBytes = Encoding.UTF8.GetBytes(_name);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, null, null);

            Assert.AreSame(_encoding, request.Encoding);
            Assert.AreEqual(_name, request.Name);
            Assert.AreEqual(_path, request.Path);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.Extended, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();
            IList<SftpExtendedReplyResponse> extendedReplyActionInvocations = new List<SftpExtendedReplyResponse>();

            Action<SftpExtendedReplyResponse> extendedAction = extendedReplyActionInvocations.Add;
            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, extendedAction, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
            Assert.AreEqual(0, extendedReplyActionInvocations.Count);
        }

        [TestMethod]
        public void Complete_SftpExtendedReplyResponse()
        {
            IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();
            IList<SftpExtendedReplyResponse> extendedReplyActionInvocations = new List<SftpExtendedReplyResponse>();

            Action<SftpExtendedReplyResponse> extendedAction = extendedReplyActionInvocations.Add;
            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var extendedReplyResponse = new SftpExtendedReplyResponse(_protocolVersion);

            var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, extendedAction, statusAction);

            request.Complete(extendedReplyResponse);

            Assert.AreEqual(0, statusActionInvocations.Count);
            Assert.AreEqual(1, extendedReplyActionInvocations.Count);
            Assert.AreSame(extendedReplyResponse, extendedReplyActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new StatVfsRequest(_protocolVersion, _requestId, _path, _encoding, null, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Name length
            expectedBytesLength += _nameBytes.Length; // Name
            expectedBytesLength += 4; // Path length
            expectedBytesLength += _pathBytes.Length; // Path

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.Extended, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());
            Assert.AreEqual((uint) _nameBytes.Length, sshDataStream.ReadUInt32());

            var actualNameBytes = new byte[_nameBytes.Length];
            sshDataStream.Read(actualNameBytes, 0, actualNameBytes.Length);
            Assert.IsTrue(_nameBytes.SequenceEqual(actualNameBytes));

            Assert.AreEqual((uint) _pathBytes.Length, sshDataStream.ReadUInt32());

            var actualPath = new byte[_pathBytes.Length];
            sshDataStream.Read(actualPath, 0, actualPath.Length);
            Assert.IsTrue(_pathBytes.SequenceEqual(actualPath));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}