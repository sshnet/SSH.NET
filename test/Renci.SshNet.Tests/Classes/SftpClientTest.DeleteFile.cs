using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest
    {
        [TestMethod]
        public void Test_Sftp_DeleteFile_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                Assert.ThrowsExactly<ArgumentNullException>(() => sftp.DeleteFile(null));
            }
        }
    }
}
