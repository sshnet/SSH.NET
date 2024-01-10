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
    public class SftpFSetStatRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private byte[] _handle;
        private SftpFileAttributes _attributes;
        private byte[] _attributesBytes;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _handle = new byte[random.Next(1, 10)];
            random.NextBytes(_handle);
            _attributes = SftpFileAttributes.Empty;
            _attributesBytes = _attributes.GetBytes();
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpFSetStatRequest(_protocolVersion, _requestId, _handle, _attributes, null);

            Assert.AreSame(_handle, request.Handle);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.FSetStat, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpFSetStatRequest(_protocolVersion, _requestId, _handle, _attributes, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpFSetStatRequest(_protocolVersion, _requestId, _handle, _attributes, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Handle length
            expectedBytesLength += _handle.Length; // Handle
            expectedBytesLength += _attributesBytes.Length; // Attributes

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.FSetStat, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _handle.Length, sshDataStream.ReadUInt32());
            var actualHandle = new byte[_handle.Length];
            sshDataStream.Read(actualHandle, 0, actualHandle.Length);
            Assert.IsTrue(_handle.SequenceEqual(actualHandle));

            var actualAttributes = new byte[_attributesBytes.Length];
            sshDataStream.Read(actualAttributes, 0, actualAttributes.Length);
            Assert.IsTrue(_attributesBytes.SequenceEqual(actualAttributes));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}