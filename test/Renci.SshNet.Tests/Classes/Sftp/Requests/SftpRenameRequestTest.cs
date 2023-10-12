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
    public class SftpRenameRequestTest : TestBase
    {
        private uint _protocolVersion;
        private uint _requestId;
        private Encoding _encoding;
        private string _oldPath;
        private byte[] _oldPathBytes;
        private string _newPath;
        private byte[] _newPathBytes;

        protected override void OnInit()
        {
            var random = new Random();

            _protocolVersion = (uint) random.Next(0, int.MaxValue);
            _requestId = (uint) random.Next(0, int.MaxValue);
            _encoding = Encoding.Unicode;
            _oldPath = random.Next().ToString(CultureInfo.InvariantCulture);
            _oldPathBytes = _encoding.GetBytes(_oldPath);
            _newPath = random.Next().ToString(CultureInfo.InvariantCulture);
            _newPathBytes = _encoding.GetBytes(_newPath);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpRenameRequest(_protocolVersion, _requestId, _oldPath, _newPath, _encoding, null);

            Assert.AreSame(_encoding, request.Encoding);
            Assert.AreEqual(_newPath, request.NewPath);
            Assert.AreEqual(_oldPath, request.OldPath);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.Rename, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpRenameRequest(_protocolVersion, _requestId, _oldPath, _newPath, _encoding, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpRenameRequest(_protocolVersion, _requestId, _oldPath, _newPath, _encoding, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // OldPath length
            expectedBytesLength += _oldPathBytes.Length; // OldPath
            expectedBytesLength += 4; // NewPath length
            expectedBytesLength += _newPathBytes.Length; // NewPath

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.Rename, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _oldPathBytes.Length, sshDataStream.ReadUInt32());
            var actualOldPath = new byte[_oldPathBytes.Length];
            sshDataStream.Read(actualOldPath, 0, actualOldPath.Length);
            Assert.IsTrue(_oldPathBytes.SequenceEqual(actualOldPath));

            Assert.AreEqual((uint) _newPathBytes.Length, sshDataStream.ReadUInt32());
            var actualNewPath = new byte[_newPathBytes.Length];
            sshDataStream.Read(actualNewPath, 0, actualNewPath.Length);
            Assert.IsTrue(_newPathBytes.SequenceEqual(actualNewPath));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}