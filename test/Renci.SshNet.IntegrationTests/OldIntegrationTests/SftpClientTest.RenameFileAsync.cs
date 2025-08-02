namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_RenameFileAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(default);

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName1 = Path.GetRandomFileName();
                string remoteFileName2 = Path.GetRandomFileName();

                CreateTestFile(uploadedFileName, 1);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    using (Stream remoteStream = await sftp.OpenAsync(remoteFileName1, FileMode.CreateNew, FileAccess.Write, default))
                    {
                        await file.CopyToAsync(remoteStream, 81920, default);
                    }
                }

                await sftp.RenameFileAsync(remoteFileName1, remoteFileName2, default);

                File.Delete(uploadedFileName);

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to RenameFile.")]
        public async Task Test_Sftp_RenameFileAsync_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(default);

                await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => sftp.RenameFileAsync(null, null, default));
            }
        }
    }
}
