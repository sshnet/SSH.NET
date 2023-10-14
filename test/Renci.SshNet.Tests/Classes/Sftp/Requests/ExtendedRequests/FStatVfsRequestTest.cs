using System;
using System.Collections.Generic;
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
    public class FStatVfsRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private byte[] _handle;
        private string _name;
        private byte[] _nameBytes;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();
            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _handle = new byte[random.Next(1, 10)];
            random.NextBytes(_handle);

            _name = "fstatvfs@openssh.com";
            _nameBytes = Encoding.UTF8.GetBytes(_name);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, null, null);

            Assert.AreSame(_handle, request.Handle);
            Assert.AreEqual(_name, request.Name);
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

            var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, extendedAction, statusAction);

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

            var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, extendedAction, statusAction);

            request.Complete(extendedReplyResponse);

            Assert.AreEqual(0, statusActionInvocations.Count);
            Assert.AreEqual(1, extendedReplyActionInvocations.Count);
            Assert.AreSame(extendedReplyResponse, extendedReplyActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new FStatVfsRequest(_protocolVersion, _requestId, _handle, null, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Name length
            expectedBytesLength += _nameBytes.Length; // Name
            expectedBytesLength += 4; // Handle length
            expectedBytesLength += _handle.Length; // Handle

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.Extended, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());
            Assert.AreEqual((uint) _nameBytes.Length, sshDataStream.ReadUInt32());

            var actualNameBytes = new byte[_nameBytes.Length];
            sshDataStream.Read(actualNameBytes, 0, actualNameBytes.Length);
            Assert.IsTrue(_nameBytes.SequenceEqual(actualNameBytes));

            Assert.AreEqual((uint) _handle.Length, sshDataStream.ReadUInt32());

            var actualHandle = new byte[_handle.Length];
            sshDataStream.Read(actualHandle, 0, actualHandle.Length);
            Assert.IsTrue(_handle.SequenceEqual(actualHandle));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}