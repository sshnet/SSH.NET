using System.Diagnostics;

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
        public void Test_Sftp_ListDirectory_Permission_Denied()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPermissionDeniedException>(() => sftp.ListDirectory("/root"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ListDirectory_Not_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<SftpPathNotFoundException>(() => sftp.ListDirectory("/asdfgh"));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ListDirectory_Current()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var files = sftp.ListDirectory(".");

                Assert.IsTrue(files.Count() > 0);

                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

#if NET
        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_ListDirectoryAsync_Current()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(1));
                var count = 0;
                await foreach (var file in sftp.ListDirectoryAsync(".", cts.Token))
                {
                    count++;
                    Debug.WriteLine(file.FullName);
                }

                Assert.IsTrue(count > 0);

                sftp.Disconnect();
            }
        }
#endif
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ListDirectory_Empty()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var files = sftp.ListDirectory(string.Empty);

                Assert.IsTrue(files.Count() > 0);

                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to ListDirectory.")]
        public void Test_Sftp_ListDirectory_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.ListDirectory(null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ListDirectory_HugeDirectory()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                //  Create 10000 directory items
                for (int i = 0; i < 10000; i++)
                {
                    sftp.CreateDirectory(string.Format("test_{0}", i));
                }

                var files = sftp.ListDirectory(".");

                //  Ensure that directory has at least 10000 items
                Assert.IsTrue(files.Count() > 10000);

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Change_Directory()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.AreEqual("/home/sshnet", sftp.WorkingDirectory);

                sftp.CreateDirectory("test1");

                sftp.ChangeDirectory("test1");

                Assert.AreEqual("/home/sshnet/test1", sftp.WorkingDirectory);

                sftp.CreateDirectory("test1_1");
                sftp.CreateDirectory("test1_2");
                sftp.CreateDirectory("test1_3");

                var files = sftp.ListDirectory(".");

                Assert.IsTrue(files.First().FullName.StartsWith(string.Format("{0}", sftp.WorkingDirectory)));

                sftp.ChangeDirectory("test1_1");

                Assert.AreEqual("/home/sshnet/test1/test1_1", sftp.WorkingDirectory);

                sftp.ChangeDirectory("../test1_2");

                Assert.AreEqual("/home/sshnet/test1/test1_2", sftp.WorkingDirectory);

                sftp.ChangeDirectory("..");

                Assert.AreEqual("/home/sshnet/test1", sftp.WorkingDirectory);

                sftp.ChangeDirectory("..");

                Assert.AreEqual("/home/sshnet", sftp.WorkingDirectory);

                files = sftp.ListDirectory("test1/test1_1");

                Assert.IsTrue(files.First().FullName.StartsWith(string.Format("{0}/test1/test1_1", sftp.WorkingDirectory)));

                sftp.ChangeDirectory("test1/test1_1");

                Assert.AreEqual("/home/sshnet/test1/test1_1", sftp.WorkingDirectory);

                sftp.ChangeDirectory("/home/sshnet/test1/test1_1");

                Assert.AreEqual("/home/sshnet/test1/test1_1", sftp.WorkingDirectory);

                sftp.ChangeDirectory("/home/sshnet/test1/test1_1/../test1_2");

                Assert.AreEqual("/home/sshnet/test1/test1_2", sftp.WorkingDirectory);

                sftp.ChangeDirectory("../../");

                sftp.DeleteDirectory("test1/test1_1");
                sftp.DeleteDirectory("test1/test1_2");
                sftp.DeleteDirectory("test1/test1_3");
                sftp.DeleteDirectory("test1");

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_Change_DirectoryAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet", sftp.WorkingDirectory);

                await sftp.CreateDirectoryAsync("test1", CancellationToken.None).ConfigureAwait(false);

                await sftp.ChangeDirectoryAsync("test1", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test1", sftp.WorkingDirectory);

                await sftp.CreateDirectoryAsync("test1_1", CancellationToken.None).ConfigureAwait(false);
                await sftp.CreateDirectoryAsync("test1_2", CancellationToken.None).ConfigureAwait(false);
                await sftp.CreateDirectoryAsync("test1_3", CancellationToken.None).ConfigureAwait(false);

                var files = sftp.ListDirectory(".");

                Assert.IsTrue(files.First().FullName.StartsWith(string.Format("{0}", sftp.WorkingDirectory)));

                await sftp.ChangeDirectoryAsync("test1_1", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test1/test1_1", sftp.WorkingDirectory);

                await sftp.ChangeDirectoryAsync("../test1_2", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test1/test1_2", sftp.WorkingDirectory);

                await sftp.ChangeDirectoryAsync("..", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test1", sftp.WorkingDirectory);

                await sftp.ChangeDirectoryAsync("..", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet", sftp.WorkingDirectory);

                files = sftp.ListDirectory("test1/test1_1");

                Assert.IsTrue(files.First().FullName.StartsWith(string.Format("{0}/test1/test1_1", sftp.WorkingDirectory)));

                await sftp.ChangeDirectoryAsync("test1/test1_1", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test1/test1_1", sftp.WorkingDirectory);

                await sftp.ChangeDirectoryAsync("/home/sshnet/test1/test1_1", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test1/test1_1", sftp.WorkingDirectory);

                await sftp.ChangeDirectoryAsync("/home/sshnet/test1/test1_1/../test1_2", CancellationToken.None).ConfigureAwait(false);

                Assert.AreEqual("/home/sshnet/test1/test1_2", sftp.WorkingDirectory);

                await sftp.ChangeDirectoryAsync("../../", CancellationToken.None).ConfigureAwait(false);

                await sftp.DeleteDirectoryAsync("test1/test1_1", CancellationToken.None).ConfigureAwait(false);
                await sftp.DeleteDirectoryAsync("test1/test1_2", CancellationToken.None).ConfigureAwait(false);
                await sftp.DeleteDirectoryAsync("test1/test1_3", CancellationToken.None).ConfigureAwait(false);
                await sftp.DeleteDirectoryAsync("test1", CancellationToken.None).ConfigureAwait(false);

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to ChangeDirectory.")]
        public void Test_Sftp_ChangeDirectory_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.ChangeDirectory(null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to ChangeDirectory.")]
        public async Task Test_Sftp_ChangeDirectory_NullAsync()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                await sftp.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => sftp.ChangeDirectoryAsync(null));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test calling EndListDirectory method more than once.")]
        public void Test_Sftp_Call_EndListDirectory_Twice()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                var ar = sftp.BeginListDirectory("/", null, null);
                var result = sftp.EndListDirectory(ar);

                // TODO there is no reason that this should throw
                Assert.ThrowsExactly<ArgumentException>(() => sftp.EndListDirectory(ar));
            }
        }
    }
}
