using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    [TestClass]
    public partial class SftpClientTest : TestBase
    {
        protected override void OnInit()
        {
            base.OnInit();
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.RunCommand("rm -rf *");
                client.Disconnect();
            }
        }

        protected static string CalculateMD5(string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
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