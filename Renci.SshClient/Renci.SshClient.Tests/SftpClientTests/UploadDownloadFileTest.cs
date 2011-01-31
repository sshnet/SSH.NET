using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshClient.Common;
using Renci.SshClient.Tests.Properties;
using System.IO;
using System.Security.Cryptography;
using Renci.SshClient.Sftp;
using System.Threading;

namespace Renci.SshClient.Tests.SftpClientTests
{
    [TestClass]
    public class UploadDownloadFileTest
    {

        [TestInitialize()]
        public void CleanCurrentFolder()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.RunCommand("rm -rf *");
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Upload_And_Download_1MB_File()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName = Path.GetRandomFileName();

                this.CreateTestFile(uploadedFileName, 1);

                //  Calculate has value
                var uploadedHash = CalculateMD5(uploadedFileName);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    sftp.UploadFile(file, remoteFileName);
                }

                string downloadedFileName = Path.GetTempFileName();

                using (var file = File.OpenWrite(downloadedFileName))
                {
                    sftp.DownloadFile(remoteFileName, file);
                }

                var downloadedHash = CalculateMD5(downloadedFileName);

                sftp.DeleteFile(remoteFileName);

                File.Delete(uploadedFileName);
                File.Delete(downloadedFileName);

                sftp.Disconnect();

                Assert.AreEqual(uploadedHash, downloadedHash);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Upload_Forbidden()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName = "/etc/audit/ddd";

                this.CreateTestFile(uploadedFileName, 1);
                var exceptionOccured = false;

                try
                {
                    using (var file = File.OpenRead(uploadedFileName))
                    {
                        sftp.UploadFile(file, remoteFileName);
                    }
                }
                catch (SshPermissionDeniedException)
                {
                    exceptionOccured = true;
                }

                sftp.Disconnect();

                Assert.IsTrue(exceptionOccured);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Download_Forbidden()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string remoteFileName = "/root/install.log";

                var exceptionOccured = false;

                try
                {
                    using (var ms = new MemoryStream())
                    {
                        sftp.UploadFile(ms, remoteFileName);
                    }
                }
                catch (SshPermissionDeniedException)
                {
                    exceptionOccured = true;
                }

                sftp.Disconnect();

                Assert.IsTrue(exceptionOccured);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Download_File_Not_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string remoteFileName = "/xxx/eee/yyy";

                var exceptionOccured = false;

                try
                {
                    using (var ms = new MemoryStream())
                    {
                        sftp.UploadFile(ms, remoteFileName);
                    }
                }
                catch (SshFileNotFoundException)
                {
                    exceptionOccured = true;
                }

                sftp.Disconnect();

                Assert.IsTrue(exceptionOccured);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Multiple_Async_Upload_And_Download_10Files_5MB_Each()
        {
            var maxFiles = 10;
            var maxSize = 5;

            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var testInfoList = new Dictionary<string, TestInfo>();

                for (int i = 0; i < maxFiles; i++)
                {
                    var testInfo = new TestInfo();
                    testInfo.UploadedFileName = Path.GetTempFileName();
                    testInfo.DownloadedFileName = Path.GetTempFileName();
                    testInfo.RemoteFileName = Path.GetRandomFileName();

                    this.CreateTestFile(testInfo.UploadedFileName, maxSize);

                    //  Calculate hash value
                    testInfo.UploadedHash = CalculateMD5(testInfo.UploadedFileName);

                    testInfoList.Add(testInfo.RemoteFileName, testInfo);
                }

                var uploadWaitHandles = new List<WaitHandle>();

                //  Start file uploads
                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];
                    testInfo.UploadedFile = File.OpenRead(testInfo.UploadedFileName);
                    testInfo.UploadResult = sftp.BeginUploadFile(testInfo.UploadedFile, remoteFile, null, null);


                    uploadWaitHandles.Add(testInfo.UploadResult.AsyncWaitHandle);
                }

                //  Wait for upload to finish
                bool uploadCompleted = false;
                while (!uploadCompleted)
                {
                    //  Assume upload completed
                    uploadCompleted = true;

                    foreach (var testInfo in testInfoList.Values)
                    {
                        SftpAsyncResult sftpResult = testInfo.UploadResult as SftpAsyncResult;

                        testInfo.UploadedBytes = sftpResult.UploadedBytes;

                        if (!testInfo.UploadResult.IsCompleted)
                        {
                            uploadCompleted = false;
                        }
                    }
                    Thread.Sleep(500);
                }

                //  End file uploads
                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];

                    sftp.EndUploadFile(testInfo.UploadResult);
                    testInfo.UploadedFile.Dispose();
                }

                //  Start file downloads

                var downloadWaitHandles = new List<WaitHandle>();

                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];
                    testInfo.DownloadedFile = File.OpenWrite(testInfo.DownloadedFileName);
                    testInfo.DownloadResult = sftp.BeginDownloadFile(remoteFile, testInfo.DownloadedFile, null, null);

                    downloadWaitHandles.Add(testInfo.DownloadResult.AsyncWaitHandle);
                }

                //  Wait for download to finish
                bool downloadCompleted = false;
                while (!downloadCompleted)
                {
                    //  Assume download completed
                    downloadCompleted = true;

                    foreach (var testInfo in testInfoList.Values)
                    {
                        SftpAsyncResult sftpResult = testInfo.DownloadResult as SftpAsyncResult;

                        testInfo.DownloadedBytes = sftpResult.DownloadedBytes;

                        if (!testInfo.DownloadResult.IsCompleted)
                        {
                            downloadCompleted = false;
                        }
                    }
                    Thread.Sleep(500);
                }

                var hashMatches = true;
                var uploadDownloadSizeOk = true;
                //  End file downloads
                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];

                    sftp.EndDownloadFile(testInfo.DownloadResult);

                    testInfo.DownloadedFile.Dispose();

                    testInfo.DownloadedHash = CalculateMD5(testInfo.DownloadedFileName);

                    if (!(testInfo.UploadedBytes > 0 && testInfo.DownloadedBytes > 0 && testInfo.DownloadedBytes == testInfo.UploadedBytes))
                    {
                        uploadDownloadSizeOk = false;
                    }

                    if (!testInfo.DownloadedHash.Equals(testInfo.UploadedHash))
                    {
                        hashMatches = false;
                    }
                }

                //  Clean up after test
                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];

                    sftp.DeleteFile(remoteFile);

                    File.Delete(testInfo.UploadedFileName);
                    File.Delete(testInfo.DownloadedFileName);
                }

                sftp.Disconnect();

                Assert.IsTrue(hashMatches, "Hash does not match");
                Assert.IsTrue(uploadDownloadSizeOk, "Uploaded and downloaded bytes does not match");
            }
        }

        /// <summary>
        /// Creates the test file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="size">Size in megabytes.</param>
        private void CreateTestFile(string fileName, int size)
        {
            using (var testFile = File.Create(fileName))
            {

                var random = new Random();
                for (int i = 0; i < 1024 * size; i++)
                {
                    var buffer = new byte[1024];
                    random.NextBytes(buffer);
                    testFile.Write(buffer, 0, buffer.Length);
                }
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

            public ulong UploadedBytes { get; set; }

            public ulong DownloadedBytes { get; set; }

            public FileStream UploadedFile { get; set; }

            public FileStream DownloadedFile { get; set; }

            public string UploadedHash { get; set; }

            public string DownloadedHash { get; set; }

            public IAsyncResult UploadResult { get; set; }

            public IAsyncResult DownloadResult { get; set; }
        }
    }
}
