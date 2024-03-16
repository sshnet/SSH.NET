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
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_DeleteDirectory_Which_Doesnt_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                sftp.DeleteDirectory("abcdef");

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_DeleteDirectory_Which_No_Permissions()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, AdminUser.UserName, AdminUser.Password))
            {
                sftp.Connect();

                sftp.DeleteDirectory("/usr");

                sftp.Disconnect();
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
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_DeleteDirectory_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                sftp.DeleteDirectory(null);

                sftp.Disconnect();
            }
        }
    }
}
