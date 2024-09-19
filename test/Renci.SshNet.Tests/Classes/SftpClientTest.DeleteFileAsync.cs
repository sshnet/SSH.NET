using System;
using System.Threading.Tasks;

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
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Test_Sftp_DeleteFileAsync_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                await sftp.DeleteFileAsync(null, default);
            }
        }
    }
}
