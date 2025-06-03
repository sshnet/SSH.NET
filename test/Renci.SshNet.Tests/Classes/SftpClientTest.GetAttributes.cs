using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    public partial class SftpClientTest
    {
        [TestMethod]
        public void GetAttributes_Throws_WhenNotConnected()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                Assert.ThrowsException<SshConnectionException>(() => sftp.GetAttributes("."));
            }
        }

        [TestMethod]
        public void GetAttributes_Throws_WhenDisposed()
        {
            var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD);
            sftp.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => sftp.GetAttributes("."));
        }
    }
}
