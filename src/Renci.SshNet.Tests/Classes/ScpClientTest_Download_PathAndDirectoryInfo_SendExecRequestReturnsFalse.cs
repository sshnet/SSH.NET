using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ScpClientTest_Download_PathAndDirectoryInfo_SendExecRequestReturnsFalse
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private Mock<PipeStream> _pipeStreamMock;
        private ConnectionInfo _connectionInfo;
        private ScpClient _scpClient;
        private DirectoryInfo _directoryInfo;
        private string _path;
        private string _quotedPath;
        private IList<ScpUploadEventArgs> _uploadingRegister;
        private SshException _actualException;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        protected void Arrange()
        {
            var random = new Random();
            _connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));
            _directoryInfo = new DirectoryInfo("destination");
            _path = "/home/sshnet/" + random.Next().ToString(CultureInfo.InvariantCulture);
            _quotedPath = _path.ShellQuote();
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
                .Setup(p => p.SendExecRequest(string.Format("scp -prf {0}", _quotedPath))).Returns(false);
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
                _scpClient.Download(_path, _directoryInfo);
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
        public void SendExecRequestOnChannelSessionShouldBeInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.SendExecRequest(string.Format("scp -prf {0}", _quotedPath)), Times.Once);
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
    }
}
