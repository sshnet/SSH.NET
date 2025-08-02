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
        public async Task Test_Sftp_ListDirectoryAsync_Without_ConnectingAsync()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                await Assert.ThrowsExactlyAsync<SshConnectionException>(async () =>
                {
                    await foreach (var x in sftp.ListDirectoryAsync(".", CancellationToken.None))
                    {
                    }
                });
            }
        }
    }
}
