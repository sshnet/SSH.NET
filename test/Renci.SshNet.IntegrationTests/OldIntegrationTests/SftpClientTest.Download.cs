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
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_Download_Forbidden()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, AdminUser.UserName, AdminUser.Password))
            {
                sftp.Connect();

                string remoteFileName = "/root/.profile";

                using (var ms = new MemoryStream())
                {
                    sftp.DownloadFile(remoteFileName, ms);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_Download_File_Not_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                string remoteFileName = "/xxx/eee/yyy";
                using (var ms = new MemoryStream())
                {
                    sftp.DownloadFile(remoteFileName, ms);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_BeginDownloadFile_StreamIsNull()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                sftp.BeginDownloadFile("aaaa", null, null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_BeginDownloadFile_FileNameIsWhiteSpace()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                sftp.BeginDownloadFile("   ", new MemoryStream(), null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_BeginDownloadFile_FileNameIsNull()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                sftp.BeginDownloadFile(null, new MemoryStream(), null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(ArgumentException))]
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
                sftp.EndDownloadFile(async1);
            }
        }
    }
}
