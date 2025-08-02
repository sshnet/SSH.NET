using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    public partial class SftpClientTest
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_GetAttributes_Not_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.GetAttributes("/asdfgh"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_GetAttributes_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.GetAttributes(null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_GetAttributes_Current()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var attributes = sftp.GetAttributes(".");

                Assert.IsNotNull(attributes);

                sftp.Disconnect();
            }
        }
    }
}
