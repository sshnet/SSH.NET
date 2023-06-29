using System.Text;

using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SFTP client integration tests
    /// </summary>
    [TestClass]
    public class SftpClientTests : IntegrationTestBase, IDisposable
    {
        private readonly SftpClient _sftpClient;

        public SftpClientTests()
        {
            _sftpClient = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            _sftpClient.Connect();
        }

        [TestMethod]
        public void Test_Sftp_ListDirectory_Home_Directory()
        {
            var builder = new StringBuilder();
            var files = _sftpClient.ListDirectory("/");
            foreach (var file in files)
            {
                builder.AppendLine($"{file.FullName}");
            }

            Assert.AreEqual(@"/usr
/var
/.
/bin
/mnt
/opt
/tmp
/etc
/root
/media
/..
/dev
/proc
/sys
/home
/lib
/sbin
/run
/srv
/.dockerenv
", builder.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(SftpPermissionDeniedException), "Permission denied")]
        public void Test_Sftp_ListDirectory_Permission_Denied()
        {
            _sftpClient.ListDirectory("/root");
        }

        public void Dispose()
        {
            _sftpClient.Disconnect();
            _sftpClient.Dispose();
        }
    }
}
