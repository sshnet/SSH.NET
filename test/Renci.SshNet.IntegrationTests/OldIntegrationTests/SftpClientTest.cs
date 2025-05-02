using System.Security.Cryptography;

using Renci.SshNet.Sftp;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    [TestClass]
    public partial class SftpClientTest
    {
        protected static string CalculateMD5(string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                byte[] hash;
                using (var md5 = MD5.Create())
                {
                    hash = md5.ComputeHash(file);
                }

                file.Close();

                StringBuilder sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private void RemoveAllFiles()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                client.RunCommand("rm -rf *");
                client.Disconnect();
            }
        }

        /// <summary>
        /// Helper class to help with upload and download testing
        /// </summary>
        private class TestInfo
        {
            public string RemoteFileName { get; set; }

            public string UploadedFileName { get; set; }

            public string DownloadedFileName { get; set; }

            //public ulong UploadedBytes { get; set; }

            //public ulong DownloadedBytes { get; set; }

            public FileStream UploadedFile { get; set; }

            public FileStream DownloadedFile { get; set; }

            public string UploadedHash { get; set; }

            public string DownloadedHash { get; set; }

            public SftpUploadAsyncResult UploadResult { get; set; }

            public SftpDownloadAsyncResult DownloadResult { get; set; }
        }
    }
}
