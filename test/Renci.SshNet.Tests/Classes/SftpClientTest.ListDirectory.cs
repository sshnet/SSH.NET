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
        public void Test_Sftp_ListDirectory_Without_Connecting()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                Assert.ThrowsExactly<SshConnectionException>(() => sftp.ListDirectory("."));
            }
        }
    }
}
