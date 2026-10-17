using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Verifies that a recursive download aborts with a <see cref="ScpException"/> when the
    /// server sends a file/directory name in the SCP stream that would write outside of the
    /// caller-supplied destination directory.
    /// </summary>
    [TestClass]
    public class ScpClientTest_Download_PathAndDirectoryInfo_ServerSendsUnsafeName : ScpClientTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ScpClient _scpClient;
        private DirectoryInfo _destination;
        private string _destinationRoot;
        private string _path;
        private string _transformedPath;
        private PipeStream _pipeStream;
        private Exception _actualException;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", 22, "user", new PasswordAuthenticationMethod("user", "pwd"));

            // A real, isolated destination directory so that, were the guard absent, an escape
            // would be observable rather than silently corrupting the test working directory.
            _destinationRoot = Path.Combine(Path.GetTempPath(), "sshnet-scp-guard-" + Guid.NewGuid().ToString("N"));
            _destination = Directory.CreateDirectory(_destinationRoot);

            _path = "/home/sshnet/remote";
            _transformedPath = "transformed";

            // The very first SCP record is a C (file) record whose server-controlled name is the
            // parent-directory reference "..". Without the guard this is combined into a local
            // path and opened for writing; with the guard it must be rejected up-front.
            _pipeStream = new PipeStream();
            var record = Encoding.ASCII.GetBytes("C0644 0 ..\n");
            _pipeStream.Write(record, 0, record.Length);
        }

        protected override void SetupMocks()
        {
            _ = ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                                  .Returns(SocketFactoryMock.Object);
            _ = ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                                  .Returns(SessionMock.Object);
            _ = SessionMock.Setup(p => p.Connect());
            _ = ServiceFactoryMock.Setup(p => p.CreatePipeStream())
                                  .Returns(_pipeStream);
            _ = SessionMock.Setup(p => p.CreateChannelSession())
                           .Returns(_channelSessionMock.Object);
            _ = _channelSessionMock.Setup(p => p.Open());
            _ = _remotePathTransformationMock.Setup(p => p.Transform(_path))
                                             .Returns(_transformedPath);
            _ = _channelSessionMock.Setup(p => p.SendExecRequest("scp -prf " + _transformedPath))
                                   .Returns(true);
            _ = _channelSessionMock.Setup(p => p.SendData(It.IsAny<byte[]>()));
            _ = _channelSessionMock.Setup(p => p.Dispose());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _scpClient = new ScpClient(_connectionInfo, false, ServiceFactoryMock.Object, _remotePathTransformationMock.Object);
            _scpClient.Connect();
        }

        protected override void Act()
        {
            // Capture any exception type: the guard must turn an unsafe name into a clean
            // ScpException. Without the guard the download instead proceeds to a local file
            // operation, which is exactly what this test asserts must not happen.
            try
            {
                _scpClient.Download(_path, _destination);
            }
            catch (Exception ex)
            {
                _actualException = ex;
            }
        }

        protected override void TearDown()
        {
            base.TearDown();

            _pipeStream?.Dispose();

            if (_destinationRoot != null && Directory.Exists(_destinationRoot))
            {
                Directory.Delete(_destinationRoot, recursive: true);
            }
        }

        [TestMethod]
        public void DownloadShouldHaveThrownScpException()
        {
            Assert.IsNotNull(_actualException, "Download did not abort on the unsafe server name.");
            Assert.IsInstanceOfType<ScpException>(_actualException);
            Assert.Contains("not a valid local name", _actualException.Message, StringComparison.Ordinal);
        }

        [TestMethod]
        public void NothingShouldHaveBeenWritten()
        {
            // The guard rejects the name before any local file/directory is created, so the
            // destination directory must remain empty.
            Assert.IsEmpty(Directory.GetFileSystemEntries(_destinationRoot));
        }
    }
}
