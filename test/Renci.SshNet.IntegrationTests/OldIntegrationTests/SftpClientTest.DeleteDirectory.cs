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
        public void Test_Sftp_DeleteDirectory_Which_Doesnt_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.DeleteDirectory("abcdef"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_DeleteDirectory_Which_No_Permissions()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, AdminUser.UserName, AdminUser.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPermissionDeniedException>(() => sftp.DeleteDirectory("/usr"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_DeleteDirectory()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                sftp.CreateDirectory("abcdef");
                sftp.DeleteDirectory("abcdef");

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to DeleteDirectory.")]
        public void Test_Sftp_DeleteDirectory_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.DeleteDirectory(null));
            }
        }
    }
}
