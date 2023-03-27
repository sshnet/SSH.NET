#if FEATURE_TAP
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Properties;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest
    {
        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public async Task Test_Sftp_RenameFileAsync()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                await sftp.ConnectAsync(default);

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName1 = Path.GetRandomFileName();
                string remoteFileName2 = Path.GetRandomFileName();

                this.CreateTestFile(uploadedFileName, 1);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    using (Stream remoteStream = await sftp.OpenAsync(remoteFileName1, FileMode.CreateNew, FileAccess.Write, default))
                    {
                        await file.CopyToAsync(remoteStream, 81920, default);
                    }
                }

                await sftp.RenameFileAsync(remoteFileName1, remoteFileName2, default);

                File.Delete(uploadedFileName);

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [Description("Test passing null to RenameFile.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Test_Sftp_RenameFileAsync_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                await sftp.ConnectAsync(default);

                await sftp.RenameFileAsync(null, null, default);

                sftp.Disconnect();
            }
        }
    }
}
#endif