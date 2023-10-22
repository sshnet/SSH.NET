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
        private List<ScpUploadEventArgs> _uploadingRegister;
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

            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateRemotePathDoubleQuoteTransformation())
                                   .Returns(RemotePathTransformationMock.Object);
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
                                   .Returns(PipeStreamMock.Object);
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.CreateChannelSession())
                            .Returns(ChannelSessionMock.Object);
            _ = ChannelSessionMock.InSequence(sequence)
                                   .Setup(p => p.Open());
            _ = RemotePathTransformationMock.InSequence(sequence)
                                             .Setup(p => p.Transform(_remoteDirectory))
                                             .Returns(_transformedPath);
            _ = ChannelSessionMock.InSequence(sequence)
                                   .Setup(p => p.SendExecRequest(string.Format("scp -t -d {0}", _transformedPath)))
                                   .Returns(false);
            _ = ChannelSessionMock.InSequence(sequence)
                                   .Setup(p => p.Dispose());
            _ = PipeStreamMock.InSequence(sequence)
                               .Setup(p => p.Close());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _scpClient = new ScpClient(_connectionInfo, false, ServiceFactoryMock.Object);
            _scpClient.Uploading += (sender, args) => _uploadingRegister.Add(args);
            _scpClient.Connect();
        }

        protected override void TearDown()
        {
            base.TearDown();

            _source?.Dispose();
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
            ChannelSessionMock.Verify(p => p.SendExecRequest(string.Format("scp -t -d {0}", _transformedPath)), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            ChannelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnPipeStreamShouldBeInvokedOnce()
        {
            PipeStreamMock.Verify(p => p.Close(), Times.Once);
        }

        [TestMethod]
        public void UploadingShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _uploadingRegister.Count);
        }
    }
}
