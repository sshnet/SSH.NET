namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Rename_File()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName1 = Path.GetRandomFileName();
                string remoteFileName2 = Path.GetRandomFileName();

                CreateTestFile(uploadedFileName, 1);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    sftp.UploadFile(file, remoteFileName1);
                }

                sftp.RenameFile(remoteFileName1, remoteFileName2);

                File.Delete(uploadedFileName);

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to RenameFile.")]
        public void Test_Sftp_RenameFile_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.RenameFile(null, null));
            }
        }
    }
}
