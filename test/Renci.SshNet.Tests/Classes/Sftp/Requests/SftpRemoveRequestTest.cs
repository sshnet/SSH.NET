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
    public class SftpRemoveRequestTest
    {
        private uint _protocolVersion;
        private uint _requestId;
        private Encoding _encoding;
        private string _filename;
        private byte[] _filenameBytes;

        [TestInitialize]
        public void Init()
        {
            var random = new Random();

            _protocolVersion = (uint) random.Next(0, int.MaxValue);
            _requestId = (uint) random.Next(0, int.MaxValue);
            _encoding = Encoding.Unicode;
            _filename = random.Next().ToString(CultureInfo.InvariantCulture);
            _filenameBytes = _encoding.GetBytes(_filename);
        }

        [TestMethod]
        public void Constructor()
        {
            var request = new SftpRemoveRequest(_protocolVersion, _requestId, _filename, _encoding, null);

            Assert.AreSame(_encoding, request.Encoding);
            Assert.AreEqual(_filename, request.Filename);
            Assert.AreEqual(_protocolVersion, request.ProtocolVersion);
            Assert.AreEqual(_requestId, request.RequestId);
            Assert.AreEqual(SftpMessageTypes.Remove, request.SftpMessageType);
        }

        [TestMethod]
        public void Complete_SftpStatusResponse()
        {
            var statusActionInvocations = new List<SftpStatusResponse>();

            Action<SftpStatusResponse> statusAction = statusActionInvocations.Add;
            var statusResponse = new SftpStatusResponse(_protocolVersion);

            var request = new SftpRemoveRequest(_protocolVersion, _requestId, _filename, _encoding, statusAction);

            request.Complete(statusResponse);

            Assert.AreEqual(1, statusActionInvocations.Count);
            Assert.AreSame(statusResponse, statusActionInvocations[0]);
        }

        [TestMethod]
        public void GetBytes()
        {
            var request = new SftpRemoveRequest(_protocolVersion, _requestId, _filename, _encoding, null);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 4; // Length
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // RequestId
            expectedBytesLength += 4; // Filename length
            expectedBytesLength += _filenameBytes.Length; // Filename

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual((uint) bytes.Length - 4, sshDataStream.ReadUInt32());
            Assert.AreEqual((byte) SftpMessageTypes.Remove, sshDataStream.ReadByte());
            Assert.AreEqual(_requestId, sshDataStream.ReadUInt32());

            Assert.AreEqual((uint) _filenameBytes.Length, sshDataStream.ReadUInt32());
            var actualFilename = new byte[_filenameBytes.Length];
            sshDataStream.Read(actualFilename, 0, actualFilename.Length);
            Assert.IsTrue(_filenameBytes.SequenceEqual(actualFilename));

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }
    }
}