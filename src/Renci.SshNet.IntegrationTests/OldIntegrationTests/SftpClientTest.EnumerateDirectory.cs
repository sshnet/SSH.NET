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
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_EnumerateDirectory_Without_Connecting()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                var files = sftp.EnumerateDirectory(".");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_EnumerateDirectory_Permission_Denied()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory("/root");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_EnumerateDirectory_Not_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory("/asdfgh");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void Test_Sftp_EnumerateDirectory_Current()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory(".");

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
        [TestCategory("integration")]
        public void Test_Sftp_EnumerateDirectory_Empty()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory(string.Empty);

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
        [TestCategory("integration")]
        [Description("Test passing null to EnumerateDirectory.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_EnumerateDirectory_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory(null);

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
        [TestCategory("integration")]
        public void Test_Sftp_EnumerateDirectory_HugeDirectory()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
                {
                    sftp.Connect();
                    sftp.ChangeDirectory("/home/" + User.UserName);

                    var count = 10000;
                    //  Create 10000 directory items
                    for (int i = 0; i < count; i++)
                    {
                        sftp.CreateDirectory(string.Format("test_{0}", i));
                    }
                    Debug.WriteLine(string.Format("Created {0} directories within {1} seconds", count, stopwatch.Elapsed.TotalSeconds));

                    stopwatch.Reset();
                    stopwatch.Start();
                    var files = sftp.EnumerateDirectory(".");
                    Debug.WriteLine(string.Format("Listed {0} directories within {1} seconds", count, stopwatch.Elapsed.TotalSeconds));

                    //  Ensure that directory has at least 10000 items
                    stopwatch.Reset();
                    stopwatch.Start();
                    var actualCount = files.Count();
                    Assert.IsTrue(actualCount >= count);
                    Debug.WriteLine(string.Format("Used {0} items within {1} seconds", actualCount, stopwatch.Elapsed.TotalSeconds));

                    sftp.Disconnect();
                }
            }
            finally
            {
                stopwatch.Reset();
                stopwatch.Start();
                RemoveAllFiles();
                stopwatch.Stop();
                Debug.WriteLine(string.Format("Removed all files within {0} seconds", stopwatch.Elapsed.TotalSeconds));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_EnumerateDirectory_After_Disconnected()
        {
            try
            {
                using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
                {
                    sftp.Connect();

                    sftp.CreateDirectory("test_at_dsiposed");

                    var files = sftp.EnumerateDirectory(".").Take(1);

                    sftp.Disconnect();

                    // Must fail on disconnected session.
                    var count = files.Count();
                }
            }
            finally
            {
                RemoveAllFiles();
            }
        }
    }
}
