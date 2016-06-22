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
    public class SftpBlockRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private byte[] _handle;
        private ulong _offset;
        private ulong _length;
        private uint _lockMask;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _handle = new byte[random.Next(1, 10)];
            random.NextBytes(_handle);
            _offset = (ulong) random.Next(0, int.MaxValue);
            _length = (ulong) random.Next(0, int.MaxValue);
            _lockMask = (uint) random.Next(0, int.MaxValue);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpBlockRequest(_protocolVersion, _requestId, _handle, _offset, _length, _lockMask, null);

            Assert.AreSame(_handle, request.Handle);
            Assert.AreEqual(_length, request.Length);
            Assert.AreEqual(_lockMask, request.LockMask);
            Assert.AreEqual(_offset, request.Offset);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.Block, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            IList<SftpStatusResponse> statusActionInvocations = new List<SftpStatusResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpBlockRequest(_protocolVersion, _requestId, _handle, _offset, _length, _lockMask, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpBlockRequest(_protocolVersion, _requestId, _handle, _offset, _length, _lockMask, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Handle length
            expectedBytesLength += _handle.Length; // Handle
            expectedBytesLength += 8; // Offset
            expectedBytesLength += 8; // Length
            expectedBytesLength += 4; // LockMask

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.Block, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _handle.Length, sshDataStream.ReadUInt32());
            var actualHandle = new byte[_handle.Length];
            sshDataStream.Read(actualHandle, 0, actualHandle.Length);
            Assert.IsTrue(_handle.SequenceEqual(actualHandle));

            Assert.AreEqual(_offset, sshDataStream.ReadUInt64());
            Assert.AreEqual(_length, sshDataStream.ReadUInt64());
            Assert.AreEqual(_lockMask, sshDataStream.ReadUInt32());
            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}