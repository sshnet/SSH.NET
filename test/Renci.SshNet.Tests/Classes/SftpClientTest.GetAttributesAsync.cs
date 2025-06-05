using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    public partial class SftpClientTest
    {
        [TestMethod]
        public async Task GetAttributesAsync_Throws_WhenNotConnected()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                await Assert.ThrowsExceptionAsync<SshConnectionException>(() => sftp.GetAttributesAsync(".", CancellationToken.None));
            }
        }

        [TestMethod]
        public async Task GetAttributesAsync_Throws_WhenDisposed()
        {
            var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD);
            sftp.Dispose();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => sftp.GetAttributesAsync(".", CancellationToken.None));
        }
    }
}
