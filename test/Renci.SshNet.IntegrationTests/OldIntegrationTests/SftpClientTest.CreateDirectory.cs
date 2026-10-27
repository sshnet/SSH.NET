
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_CreateDirectory_In_Current_Location()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                sftp.CreateDirectory("test-in-current");

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_CreateDirectory_In_Forbidden_Directory()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, AdminUser.UserName, AdminUser.Password))
            {
                sftp.Connect();

                var ex = Assert.ThrowsExactly<SftpPermissionDeniedException>(() => sftp.CreateDirectory("/sbin/test"));
                Assert.AreEqual("/sbin/test", ex.Path);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_CreateDirectory_Invalid_Path()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var ex = Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.CreateDirectory("/abcdefg/abcefg"));
                Assert.AreEqual("/abcdefg/abcefg", ex.Path);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_CreateDirectory_Already_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                sftp.CreateDirectory("test");

                var ex = Assert.ThrowsExactly<SftpException>(() => sftp.CreateDirectory("test"));
                Assert.AreEqual(StatusCode.Failure, ex.StatusCode);
                Assert.AreEqual($"{sftp.WorkingDirectory}/test", ex.Path);
            }
        }
    }
}
