using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ScpClientTest_Upload_StreamAndPath_SendExecRequestReturnsFalse : ScpClientTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ScpClient _scpClient;
        private Stream _source;
        private string _remoteDirectory;
        private string _remoteFile;
        private string _remotePath;
        private string _transformedPath;
        private IList<ScpUploadEventArgs> _uploadingRegister;
        private SshException _actualException;

        protected override void SetupData()
        {
            var random = new Random();

            _connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));
            _source = new MemoryStream();
            _remoteDirectory = "/home/sshnet";
            _remoteFile = random.Next().ToString();
            _remotePath = _remoteDirectory + "/" + _remoteFile;
            _transformedPath = random.Next().ToString();
            _uploadingRegister = new List<ScpUploadEventArgs>();
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateRemotePathDoubleQuoteTransformation())
                               .Returns(_remotePathTransformationMock.Object);
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSocketFactory())
                               .Returns(_socketFactoryMock.Object);
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo, _socketFactoryMock.Object))
                               .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence).Setup(p => p.CreatePipeStream()).Returns(_pipeStreamMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Open());
            _remotePathTransformationMock.InSequence(sequence)
                                         .Setup(p => p.Transform(_remoteDirectory))
                                         .Returns(_transformedPath);
            _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendExecRequest(string.Format("scp -t -d {0}", _transformedPath)))
                               .Returns(false);
            _channelSessionMock.InSequence(sequence).Setup(p => p.Dispose());
#if NET35
            _pipeStreamMock.As<IDisposable>().InSequence(sequence).Setup(p => p.Dispose());
#else
            _pipeStreamMock.InSequence(sequence).Setup(p => p.Close());
#endif
        }

        protected override void Arrange()
        {
            base.Arrange();

            _scpClient = new ScpClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _scpClient.Uploading += (sender, args) => _uploadingRegister.Add(args);
            _scpClient.Connect();
        }

        protected override void TearDown()
        {
            base.TearDown();

            if (_source != null)
            {
                _source.Dispose();
            }
        }

        protected override void Act()
        {
            try
            {
                _scpClient.Upload(_source, _remotePath);
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
#if NET35
            _pipeStreamMock.As<IDisposable>().Verify(p => p.Dispose(), Times.Once);
#else
            _pipeStreamMock.Verify(p => p.Close(), Times.Once);
#endif
        }

        [TestMethod]
        public void UploadingShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _uploadingRegister.Count);
        }
    }
}
