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
        public void Test_Sftp_ChangeDirectory_Root_Dont_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.ChangeDirectory("/asdasd"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_ChangeDirectory_Root_Dont_ExistsAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<SftpPathNotFoundException>(
                    () => sftp.ChangeDirectoryAsync("/asdasd", CancellationToken.None));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ChangeDirectory_Root_With_Slash_Dont_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.ChangeDirectory("/asdasd/"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_ChangeDirectory_Root_With_Slash_Dont_ExistsAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<SftpPathNotFoundException>(
                    () => sftp.ChangeDirectoryAsync("/asdasd/", CancellationToken.None));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ChangeDirectory_Subfolder_Dont_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.ChangeDirectory("/asdasd/sssddds"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_ChangeDirectory_Subfolder_Dont_ExistsAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<SftpPathNotFoundException>(
                    () => sftp.ChangeDirectoryAsync("/asdasd/sssddds", CancellationToken.None));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ChangeDirectory_Subfolder_With_Slash_Dont_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.ChangeDirectory("/asdasd/sssddds/"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_ChangeDirectory_Subfolder_With_Slash_Dont_ExistsAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<SftpPathNotFoundException>(
                    () => sftp.ChangeDirectoryAsync("/asdasd/sssddds/", CancellationToken.None));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ChangeDirectory_Which_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/usr");
                Assert.AreEqual("/usr", sftp.WorkingDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_ChangeDirectory_Which_ExistsAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
                await sftp.ChangeDirectoryAsync("/usr", CancellationToken.None).ConfigureAwait(false);
                Assert.AreEqual("/usr", sftp.WorkingDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ChangeDirectory_Which_Exists_With_Slash()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/usr/");
                Assert.AreEqual("/usr", sftp.WorkingDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_ChangeDirectory_Which_Exists_With_SlashAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
                await sftp.ChangeDirectoryAsync("/usr/", CancellationToken.None).ConfigureAwait(false);
                Assert.AreEqual("/usr", sftp.WorkingDirectory);
            }
        }
    }
}
