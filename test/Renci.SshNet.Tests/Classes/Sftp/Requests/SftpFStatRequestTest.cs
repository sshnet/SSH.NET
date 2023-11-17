using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp.Requests
{
    [TestClass]
    public class SftpFStatRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private byte[] _handle;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _handle = new byte[random.Next(1, 10)];
            random.NextBytes(_handle);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, null, null);

            Assert.AreSame(_handle, request.Handle);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.FStat, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpAttrsResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();
            var attrsActionInvocations = new List<SftpAttrsResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            Action<SftpAttrsResponse> attrsAction = attrsActionInvocations.Add;
            var attrsResponse = new SftpAttrsResponse(_protocolVersion);

            var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, attrsAction, statusAction);

            request.Complete(attrsResponse);

            Assert.AreEqual(0, statusActionInvocations.Count);
            Assert.AreEqual(1, attrsActionInvocations.Count);
            Assert.AreSame(attrsResponse, attrsActionInvocations[0]);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();
            var attrsActionInvocations = new List<SftpAttrsResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            Action<SftpAttrsResponse> attrsAction = attrsActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, attrsAction, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
            Assert.AreEqual(0, attrsActionInvocations.Count);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpFStatRequest(_protocolVersion, _requestId, _handle, null, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Handle length
            expectedBytesLength += _handle.Length; // Handle

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.FStat, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _handle.Length, sshDataStream.ReadUInt32());
            var actualHandle = new byte[_handle.Length];
            sshDataStream.Read(actualHandle, 0, actualHandle.Length);
            Assert.IsTrue(_handle.SequenceEqual(actualHandle));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}