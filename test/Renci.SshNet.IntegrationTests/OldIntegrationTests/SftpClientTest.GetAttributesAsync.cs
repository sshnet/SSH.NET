using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_GetAttributesAsync_Not_Exists()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(1));

                await sftp.ConnectAsync(cts.Token);

                await Assert.ThrowsExceptionAsync<SftpPathNotFoundException>(async () => await sftp.GetAttributesAsync("/asdfgh", cts.Token));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_GetAttributesAsync_Null()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(1));

                await sftp.ConnectAsync(cts.Token);

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await sftp.GetAttributesAsync(null, cts.Token));
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public async Task Test_Sftp_GetAttributesAsync_Current()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(1));

                await sftp.ConnectAsync(cts.Token);

                var fileAttributes = await sftp.GetAttributesAsync(".", cts.Token);

                Assert.IsNotNull(fileAttributes);

                sftp.Disconnect();
            }
        }
    }
}
