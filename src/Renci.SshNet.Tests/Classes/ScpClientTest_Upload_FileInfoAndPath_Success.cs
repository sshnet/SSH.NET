using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ScpClientTest_Upload_FileInfoAndPath_Success
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private Mock<PipeStream> _pipeStreamMock;
        private ConnectionInfo _connectionInfo;
        private ScpClient _scpClient;
        private FileInfo _fileInfo;
        private string _path;
        private string _quotedPath;
        private int _bufferSize;
        private byte[] _fileContent;
        private string _fileName;
        private int _fileSize;
        private IList<ScpUploadEventArgs> _uploadingRegister;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_fileName != null)
            {
                File.Delete(_fileName);
                _fileName = null;
            }
        }

        private void SetupData()
        {
            var random = new Random();

            _bufferSize = random.Next(5, 15);
            _fileSize = _bufferSize + 2; //force uploading 2 chunks
            _fileContent = CreateContent(_fileSize);
            _fileName = CreateTemporaryFile(_fileContent);
            _connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));
            _fileInfo = new FileInfo(_fileName);
            _path = "/home/sshnet/" + random.Next().ToString(CultureInfo.InvariantCulture);
            _quotedPath = _path.ShellQuote();
            _uploadingRegister = new List<ScpUploadEventArgs>();
        }

        private void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _pipeStreamMock = new Mock<PipeStream>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var sequence = new MockSequence();
            _serviceFactoryMock.InSequence(sequence)
                .Setup(p => p.CreateSession(_connectionInfo))
                .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence).Setup(p => p.CreatePipeStream()).Returns(_pipeStreamMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Open());
            _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendExecRequest(string.Format("scp -t {0}", _quotedPath))).Returns(true);
            _pipeStreamMock.InSequence(sequence).Setup(p => p.ReadByte()).Returns(0);
            _channelSessionMock.InSequence(sequence).Setup(p => p.SendData(It.IsAny<byte[]>()));
            _pipeStreamMock.InSequence(sequence).Setup(p => p.ReadByte()).Returns(0);
            _channelSessionMock.InSequence(sequence)
                .Setup(p => p.SendData(It.Is<byte[]>(b => b.SequenceEqual(CreateData(
                    string.Format("C0644 {0} {1}\n", _fileInfo.Length, string.Empty))))));
            _pipeStreamMock.InSequence(sequence).Setup(p => p.ReadByte()).Returns(0);
            _channelSessionMock.InSequence(sequence)
                .Setup(
                    p => p.SendData(It.Is<byte[]>(b => b.SequenceEqual(_fileContent.Take(_bufferSize))), 0, _bufferSize));
            _channelSessionMock.InSequence(sequence)
                .Setup(
                    p => p.SendData(It.Is<byte[]>(b => b.Take(0, _fileContent.Length - _bufferSize).SequenceEqual(_fileContent.Take(_bufferSize, _fileContent.Length - _bufferSize))), 0, _fileContent.Length - _bufferSize));
            _channelSessionMock.InSequence(sequence)
                .Setup(
                    p => p.SendData(It.Is<byte[]>(b => b.SequenceEqual(new byte[] {0}))));
            _pipeStreamMock.InSequence(sequence).Setup(p => p.ReadByte()).Returns(0);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Dispose());
            _pipeStreamMock.As<IDisposable>().InSequence(sequence).Setup(p => p.Dispose());
        }

        protected void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _scpClient = new ScpClient(_connectionInfo, false, _serviceFactoryMock.Object)
                {
                    BufferSize = (uint) _bufferSize
                };
            _scpClient.Uploading += (sender, args) => _uploadingRegister.Add(args);
            _scpClient.Connect();
        }

        protected virtual void Act()
        {
            _scpClient.Upload(_fileInfo, _path);
        }

        [TestMethod]
        public void SendExecRequestOnChannelSessionShouldBeInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.SendExecRequest(string.Format("scp -t {0}", _quotedPath)), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnPipeStreamShouldBeInvokedOnce()
        {
            _pipeStreamMock.As<IDisposable>().Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void UploadingShouldHaveFiredTwice()
        {
            Assert.AreEqual(2, _uploadingRegister.Count);

            var uploading = _uploadingRegister[0];
            Assert.IsNotNull(uploading);
            Assert.AreSame(_fileInfo.Name, uploading.Filename);
            Assert.AreEqual(_fileSize, uploading.Size);
            Assert.AreEqual(_bufferSize, uploading.Uploaded);

            uploading = _uploadingRegister[1];
            Assert.IsNotNull(uploading);
            Assert.AreSame(_fileInfo.Name, uploading.Filename);
            Assert.AreEqual(_fileSize, uploading.Size);
            Assert.AreEqual(_fileSize, uploading.Uploaded);
        }

        private static IEnumerable<byte> CreateData(string command)
        {
            return Encoding.Default.GetBytes(command);
        }

        private static byte[] CreateContent(int length)
        {
            var random = new Random();
            var content = new byte[length];

            for (var i = 0; i < length; i++)
                content[i] = (byte) random.Next(byte.MinValue, byte.MaxValue);
            return content;
        }

        private static string CreateTemporaryFile(byte[] content)
        {
            var tempFile = Path.GetTempFileName();
            using (var fs = File.OpenWrite(tempFile))
            {
                fs.Write(content, 0, content.Length);
            }
            return tempFile;
        }
    }
}
