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
    public class SftpSymLinkRequestTest : TestBase
    {
        private uint _protocolVersion;
        private uint _requestId;
        private Encoding _encoding;
        private string _newLinkPath;
        private byte[] _newLinkPathBytes;
        private string _existingPath;
        private byte[] _existingPathBytes;

        protected override void OnInit()
        {
            var random = new Random();

            _protocolVersion = (uint) random.Next(0, int.MaxValue);
            _requestId = (uint) random.Next(0, int.MaxValue);
            _encoding = Encoding.Unicode;
            _newLinkPath = random.Next().ToString(CultureInfo.InvariantCulture);
            _newLinkPathBytes = _encoding.GetBytes(_newLinkPath);
            _existingPath = random.Next().ToString(CultureInfo.InvariantCulture);
            _existingPathBytes = _encoding.GetBytes(_existingPath);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpSymLinkRequest(
                _protocolVersion,
                _requestId,
                _newLinkPath,
                _existingPath,
                _encoding,
                null);

            Assert.AreSame(_encoding, request.Encoding);
            Assert.AreEqual(_existingPath, request.ExistingPath);
            Assert.AreEqual(_newLinkPath, request.NewLinkPath);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.SymLink, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpSymLinkRequest(
                _protocolVersion,
                _requestId,
                _newLinkPath,
                _existingPath,
                _encoding,
                statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpSymLinkRequest(
                _protocolVersion,
                _requestId,
                _newLinkPath,
                _existingPath,
                _encoding,
                null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // NewLinkPath length
            expectedBytesLength += _newLinkPathBytes.Length; // NewLinkPath
            expectedBytesLength += 4; // ExistingPath length
            expectedBytesLength += _existingPathBytes.Length; // ExistingPath

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.SymLink, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _newLinkPathBytes.Length, sshDataStream.ReadUInt32());
            var actualNewLinkPath = new byte[_newLinkPathBytes.Length];
            sshDataStream.Read(actualNewLinkPath, 0, actualNewLinkPath.Length);
            Assert.IsTrue(_newLinkPathBytes.SequenceEqual(actualNewLinkPath));

            Assert.AreEqual((uint) _existingPathBytes.Length, sshDataStream.ReadUInt32());
            var actualExistingPath = new byte[_existingPathBytes.Length];
            sshDataStream.Read(actualExistingPath, 0, actualExistingPath.Length);
            Assert.IsTrue(_existingPathBytes.SequenceEqual(actualExistingPath));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}