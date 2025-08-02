using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Represents SFTP file information
    /// </summary>
    [TestClass]
    public class SftpFileTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_Root_Directory()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                var directory = sftp.Get("/");

                Assert.AreEqual("/", directory.FullName);
                Assert.IsTrue(directory.IsDirectory);
                Assert.IsFalse(directory.IsRegularFile);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_Invalid_Directory()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.Get("/xyz"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_File()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                sftp.UploadFile(new MemoryStream(), "abc.txt");

                var file = sftp.Get("abc.txt");

                Assert.AreEqual("/home/sshnet/abc.txt", file.FullName);
                Assert.IsTrue(file.IsRegularFile);
                Assert.IsFalse(file.IsDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_File_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.Get(null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_International_File()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                sftp.UploadFile(new MemoryStream(), "test-üöä-");

                var file = sftp.Get("test-üöä-");

                Assert.AreEqual("/home/sshnet/test-üöä-", file.FullName);
                Assert.IsTrue(file.IsRegularFile);
                Assert.IsFalse(file.IsDirectory);
            }
        }
        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Get_Root_DirectoryAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
                var directory = await sftp.GetAsync("/", default).ConfigureAwait(false);

                Assert.AreEqual("/", directory.FullName);
                Assert.IsTrue(directory.IsDirectory);
                Assert.IsFalse(directory.IsRegularFile);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Get_Invalid_DirectoryAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<SftpPathNotFoundException>(() => sftp.GetAsync("/xyz", default));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Get_FileAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await sftp.UploadFileAsync(new MemoryStream(), "abc.txt").ConfigureAwait(false);

                var file = await sftp.GetAsync("abc.txt", default).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/abc.txt", file.FullName);
                Assert.IsTrue(file.IsRegularFile);
                Assert.IsFalse(file.IsDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Get_File_NullAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => sftp.GetAsync(null, default));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Get_International_FileAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await sftp.UploadFileAsync(new MemoryStream(), "test-üöä-").ConfigureAwait(false);

                var file = await sftp.GetAsync("test-üöä-", default).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test-üöä-", file.FullName);
                Assert.IsTrue(file.IsRegularFile);
                Assert.IsFalse(file.IsDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_SftpFile_MoveTo()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName = Path.GetRandomFileName();
                string newFileName = Path.GetRandomFileName();

                CreateTestFile(uploadedFileName, 1);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    sftp.UploadFile(file, remoteFileName);
                }

                var sftpFile = sftp.Get(remoteFileName);

                sftpFile.MoveTo(newFileName);

                Assert.AreEqual(newFileName, sftpFile.Name);

                sftp.Disconnect();
            }
        }
    }
}
