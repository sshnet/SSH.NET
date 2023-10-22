namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public sealed partial class SftpClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_RenameFileAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(default).ConfigureAwait(continueOnCapturedContext: false);

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName1 = Path.GetRandomFileName();
                string remoteFileName2 = Path.GetRandomFileName();

                CreateTestFile(uploadedFileName, 1);

#pragma warning disable MA0042 // Do not use blocking calls in an async method
                using (var file = File.OpenRead(uploadedFileName))
                {
                    using (Stream remoteStream = await sftp.OpenAsync(remoteFileName1, FileMode.CreateNew, FileAccess.Write, default).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        await file.CopyToAsync(remoteStream, 81920, default)
                                  .ConfigureAwait(continueOnCapturedContext: false);
                    }
                }
#pragma warning restore MA0042 // Do not use blocking calls in an async method

                await sftp.RenameFileAsync(remoteFileName1, remoteFileName2, default).ConfigureAwait(continueOnCapturedContext: false);

                File.Delete(uploadedFileName);

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to RenameFile.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Test_Sftp_RenameFileAsync_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(default)
                          .ConfigureAwait(continueOnCapturedContext: false);

                await sftp.RenameFileAsync(oldPath: null, newPath: null, cancellationToken: default)
                          .ConfigureAwait(false);

                sftp.Disconnect();
            }
        }
    }
}
