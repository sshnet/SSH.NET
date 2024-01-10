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

namespace Renci.SshNet.Tests.Classes.Sftp.Requests
{
    [TestClass]
    public class SftpLinkRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private string _newLinkPath;
        private byte[] _newLinkPathBytes;
        private string _existingPath;
        private byte[] _existingPathBytes;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _newLinkPath = random.Next().ToString(CultureInfo.InvariantCulture);
            _newLinkPathBytes = Encoding.UTF8.GetBytes(_newLinkPath);
            _existingPath = random.Next().ToString(CultureInfo.InvariantCulture);
            _existingPathBytes = Encoding.UTF8.GetBytes(_existingPath);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, true, null);

            Assert.AreEqual(_existingPath, request.ExistingPath);
            Assert.IsTrue(request.IsSymLink);
            Assert.AreEqual(_newLinkPath, request.NewLinkPath);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.Link, request.SftpMessageType);

            request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, false, null);

            Assert.IsFalse(request.IsSymLink);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, true, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpLinkRequest(_protocolVersion, _requestId, _newLinkPath, _existingPath, true, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // NewLinkPath length
            expectedBytesLength += _newLinkPathBytes.Length; // NewLinkPath
            expectedBytesLength += 4; // ExistingPath length
            expectedBytesLength += _existingPathBytes.Length; // ExistingPath
            expectedBytesLength += 1; // IsSymLink

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.Link, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _newLinkPathBytes.Length, sshDataStream.ReadUInt32());
            var actualNewLinkPath = new byte[_newLinkPathBytes.Length];
            sshDataStream.Read(actualNewLinkPath, 0, actualNewLinkPath.Length);
            Assert.IsTrue(_newLinkPathBytes.SequenceEqual(actualNewLinkPath));

            Assert.AreEqual((uint) _existingPathBytes.Length, sshDataStream.ReadUInt32());
            var actualExistingPath = new byte[_existingPathBytes.Length];
            sshDataStream.Read(actualExistingPath, 0, actualExistingPath.Length);
            Assert.IsTrue(_existingPathBytes.SequenceEqual(actualExistingPath));

            Assert.AreEqual(1, sshDataStream.ReadByte());

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}