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
    public class ScpClientTest_Upload_FileInfoAndPath_SendExecRequestReturnsFalse
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private Mock<PipeStream> _pipeStreamMock;
        private ConnectionInfo _connectionInfo;
        private ScpClient _scpClient;
        private FileInfo _fileInfo;
        private string _path;
        private string _fileName;
        private IList<ScpUploadEventArgs> _uploadingRegister;
        private SshException _actualException;

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

        protected void Arrange()
        {
            var random = new Random();
            _fileName = CreateTemporaryFile(new byte[] {1});
            _connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));
            _fileInfo = new FileInfo(_fileName);
            _path = random.Next().ToString(CultureInfo.InvariantCulture);
            _uploadingRegister = new List<ScpUploadEventArgs>();

            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _pipeStreamMock = new Mock<PipeStream>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _serviceFactoryMock.InSequence(sequence)
                .Setup(p => p.CreateSession(_connectionInfo))
                .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence).Setup(p => p.CreatePipeStream()).Returns(_pipeStreamMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Open());
            _channelSessionMock.InSequence(sequence)
                .Setup(
                    p => p.SendExecRequest(string.Format("scp -t \"{0}\"", _path))).Returns(false);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Dispose());
            _pipeStreamMock.As<IDisposable>().InSequence(sequence).Setup(p => p.Dispose());

            _scpClient = new ScpClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _scpClient.Uploading += (sender, args) => _uploadingRegister.Add(args);
            _scpClient.Connect();
        }

        protected virtual void Act()
        {
            try
            {
                _scpClient.Upload(_fileInfo, _path);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void UploadShouldHaveThrownSshException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("Secure copy execution request was rejected by the server. Please consult the server logs.", _actualException.Message);
        }

        [TestMethod]
        public void SendExecREquestOnChannelSessionShouldBeInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.SendExecRequest(string.Format("scp -t \"{0}\"", _path)), Times.Once);
        }

        [TestMethod]
        public void CloseOnChannelShouldNeverBeInvoked()
        {
            _channelSessionMock.Verify(p => p.Close(), Times.Never);
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
        public void UploadingShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _uploadingRegister.Count);
        }

        private string CreateTemporaryFile(byte[] content)
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
