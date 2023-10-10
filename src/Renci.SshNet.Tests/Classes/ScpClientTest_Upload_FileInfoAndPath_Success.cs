using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ScpClientTest_Upload_FileInfoAndPath_Success : ScpClientTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ScpClient _scpClient;
        private FileInfo _fileInfo;
        private string _remoteDirectory;
        private string _remoteFile;
        private string _remotePath;
        private string _transformedPath;
        private int _bufferSize;
        private byte[] _fileContent;
        private string _fileName;
        private int _fileSize;
        private IList<ScpUploadEventArgs> _uploadingRegister;

        protected override void SetupData()
        {
            var random = new Random();

            _bufferSize = random.Next(5, 15);
            _fileSize = _bufferSize + 2; //force uploading 2 chunks
            _fileContent = CreateContent(_fileSize);
            _fileName = CreateTemporaryFile(_fileContent);
            _connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));
            _fileInfo = new FileInfo(_fileName);
            _remoteDirectory = "/home/sshnet";
            _remoteFile = random.Next().ToString();
            _remotePath = _remoteDirectory + "/" + _remoteFile;
            _transformedPath = random.Next().ToString();
            _uploadingRegister = new List<ScpUploadEventArgs>();
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateRemotePathDoubleQuoteTransformation())
                                   .Returns(_remotePathTransformationMock.Object);
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSocketFactory())
                                   .Returns(SocketFactoryMock.Object);
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                                   .Returns(SessionMock.Object);
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.Connect());
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreatePipeStream())
                                   .Returns(_pipeStreamMock.Object);
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.CreateChannelSession())
                            .Returns(_channelSessionMock.Object);
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.Open());
            _ = _remotePathTransformationMock.InSequence(sequence)
                                             .Setup(p => p.Transform(_remoteDirectory))
                                             .Returns(_transformedPath);
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.SendExecRequest(string.Format("scp -t -d {0}", _transformedPath)))
                                   .Returns(true);
            _ = _pipeStreamMock.InSequence(sequence)
                               .Setup(p => p.ReadByte())
                               .Returns(0);
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.SendData(It.IsAny<byte[]>()));
            _ = _pipeStreamMock.InSequence(sequence)
                               .Setup(p => p.ReadByte())
                               .Returns(0);
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.SendData(It.Is<byte[]>(b => b.SequenceEqual(CreateData(string.Format("C0644 {0} {1}\n", _fileInfo.Length, _remoteFile), _connectionInfo.Encoding)))));
            _ = _pipeStreamMock.InSequence(sequence)
                               .Setup(p => p.ReadByte())
                               .Returns(0);
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.SendData(It.Is<byte[]>(b => b.SequenceEqual(_fileContent.Take(_bufferSize))), 0, _bufferSize));
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.SendData(It.Is<byte[]>(b => b.Take(0, _fileContent.Length - _bufferSize).SequenceEqual(_fileContent.Take(_bufferSize, _fileContent.Length - _bufferSize))), 0, _fileContent.Length - _bufferSize));
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.SendData(It.Is<byte[]>(b => b.SequenceEqual(new byte[] {0}))));
            _ = _pipeStreamMock.InSequence(sequence)
                               .Setup(p => p.ReadByte())
                               .Returns(0);
            _ = _channelSessionMock.InSequence(sequence)
                                   .Setup(p => p.Dispose());
            _ = _pipeStreamMock.InSequence(sequence)
                               .Setup(p => p.Close());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _scpClient = new ScpClient(_connectionInfo, false, ServiceFactoryMock.Object)
                {
                    BufferSize = (uint) _bufferSize
                };
            _scpClient.Uploading += (sender, args) => _uploadingRegister.Add(args);
            _scpClient.Connect();
        }

        protected override void TearDown()
        {
            base.TearDown();

            if (_fileName != null)
            {
                File.Delete(_fileName);
                _fileName = null;
            }
        }

        protected override void Act()
        {
            _scpClient.Upload(_fileInfo, _remotePath);
        }

        [TestMethod]
        public void SendExecRequestOnChannelSessionShouldBeInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.SendExecRequest(string.Format("scp -t -d {0}", _transformedPath)), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnPipeStreamShouldBeInvokedOnce()
        {
            _pipeStreamMock.Verify(p => p.Close(), Times.Once);
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

        private static IEnumerable<byte> CreateData(string command, Encoding encoding)
        {
            return encoding.GetBytes(command);
        }

        private static byte[] CreateContent(int length)
        {
            var random = new Random();
            var content = new byte[length];

            for (var i = 0; i < length; i++)
            {
                content[i] = (byte) random.Next(byte.MinValue, byte.MaxValue);
            }

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
