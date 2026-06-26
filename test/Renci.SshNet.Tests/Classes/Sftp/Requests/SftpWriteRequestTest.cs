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
    public class SftpWriteRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private byte[] _handle;
        private ulong _serverFileOffset;
        private byte[] _data;
        private int _offset;
        private int _length;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _protocolVersion = (uint)random.Next(0, int.MaxValue);
            _requestId = (uint)random.Next(0, int.MaxValue);
            _handle = new byte[random.Next(1, 10)];
            random.NextBytes(_handle);
            _serverFileOffset = (ulong)random.Next(0, int.MaxValue);
            _data = new byte[random.Next(10, 15)];
            random.NextBytes(_data);
            _offset = random.Next(0, _data.Length - 1);
            _length = random.Next(0, _data.Length - _offset);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpWriteRequest(
                _protocolVersion,
                new SftpWriteRequestBuffer(_handle, _serverFileOffset, _data.AsSpan(_offset, _length))
                {
                    RequestId = _requestId
                },
                statusAction: null);

            CollectionAssert.AreEqual(_data.Take(_offset, _length), request.Data.ToArray());
            CollectionAssert.AreEqual(_handle, request.Handle.ToArray());
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(_serverFileOffset, request.ServerFileOffset);
            Assert.AreEqual(SftpMessageTypes.Write, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();
            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpWriteRequest(
                _protocolVersion,
                new SftpWriteRequestBuffer(_handle, _serverFileOffset, _data.AsSpan(_offset, _length))
                {
                    RequestId = _requestId
                },
                statusAction);

            request.Complete(statusResponse);

            Assert.HasCount(1, statusActionInvocations);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpWriteRequest(
                _protocolVersion,
                new SftpWriteRequestBuffer(_handle, _serverFileOffset, _data.AsSpan(_offset, _length))
                {
                    RequestId = _requestId
                },
                statusAction: null);

            var bytes = ((SftpRequest)request).GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Handle length
            expectedBytesLength += _handle.Length; // Handle
            expectedBytesLength += 8; // ServerFileOffset
            expectedBytesLength += 4; // Data length
            expectedBytesLength += _length; // Data

            Assert.HasCount(expectedBytesLength, bytes);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint)bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte)SftpMessageTypes.Write, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint)_handle.Length, sshDataStream.ReadUInt32());
            var actualHandle = new byte[_handle.Length];
            _ = sshDataStream.Read(actualHandle, 0, actualHandle.Length);
            CollectionAssert.AreEqual(_handle, actualHandle);

            Assert.AreEqual(_serverFileOffset, sshDataStream.ReadUInt64());

            Assert.AreEqual((uint)_length, sshDataStream.ReadUInt32());
            var actualData = new byte[_length];
            _ = sshDataStream.Read(actualData, 0, actualData.Length);
            CollectionAssert.AreEqual(_data.Take(_offset, _length), actualData);

            Assert.IsTrue(sshDataStream.IsEndOfData);

            CollectionAssert.AreEqual(bytes, request.GetBytes().ToArray());
        }
    }
}
