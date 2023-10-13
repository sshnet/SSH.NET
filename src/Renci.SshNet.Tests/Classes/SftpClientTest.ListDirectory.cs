using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest
    {
        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_ListDirectory_Without_Connecting()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                var files = sftp.ListDirectory(".");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshConnectionException))]
        public async Task Test_Sftp_ListDirectoryAsync_Without_Connecting()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                await foreach (var file in sftp.ListDirectoryAsync(".", CancellationToken.None))
                {
                    Debug.WriteLine(file.FullName);
                }
            }
        }
    }
}
