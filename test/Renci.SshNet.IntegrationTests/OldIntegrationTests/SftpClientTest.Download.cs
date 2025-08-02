using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Download_Forbidden()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, AdminUser.UserName, AdminUser.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPermissionDeniedException>(() => sftp.DownloadFile("/root/.profile", Stream.Null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Download_File_Not_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.DownloadFile("/xxx/eee/yyy", Stream.Null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_DownloadAsync_Forbidden()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, AdminUser.UserName, AdminUser.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<SftpPermissionDeniedException>(() => sftp.DownloadFileAsync("/root/.profile", Stream.Null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_DownloadAsync_File_Not_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<SftpPathNotFoundException>(() => sftp.DownloadFileAsync("/xxx/eee/yyy", Stream.Null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_DownloadAsync_Cancellation_Requested()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                var cancelledToken = new CancellationToken(true);

                await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => sftp.DownloadFileAsync("/xxx/eee/yyy", Stream.Null, cancelledToken));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        public void Test_Sftp_BeginDownloadFile_StreamIsNull()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.BeginDownloadFile("aaaa", null, null, null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        public void Test_Sftp_BeginDownloadFile_FileNameIsWhiteSpace()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentException>(() => sftp.BeginDownloadFile("   ", Stream.Null, null, null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        public void Test_Sftp_BeginDownloadFile_FileNameIsNull()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.BeginDownloadFile(null, Stream.Null, null, null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_EndDownloadFile_Invalid_Async_Handle()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                var filename = Path.GetTempFileName();
                CreateTestFile(filename, 1);
                sftp.UploadFile(File.OpenRead(filename), "test123");
                var async1 = sftp.BeginListDirectory("/", null, null);
                var async2 = sftp.BeginDownloadFile("test123", new MemoryStream(), null, null);

                Assert.ThrowsExactly<ArgumentException>(() => sftp.EndDownloadFile(async1));
            }
        }
    }
}
