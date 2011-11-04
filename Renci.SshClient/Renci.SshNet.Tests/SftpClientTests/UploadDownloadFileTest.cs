using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;
using System.IO;
using System.Security.Cryptography;
using Renci.SshNet.Sftp;
using System.Threading;

namespace Renci.SshNet.Tests.SftpClientTests
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
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_Upload_Forbidden()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName = "/etc/audit/ddd";

                this.CreateTestFile(uploadedFileName, 1);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    sftp.UploadFile(file, remoteFileName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_Download_Forbidden()
        {
            if (Resources.USERNAME == "root")
                Assert.Fail("Must not run this test as root!");

            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string remoteFileName = "/root/install.log";

                using (var ms = new MemoryStream())
                {
                    sftp.DownloadFile(remoteFileName, ms);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_Download_File_Not_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string remoteFileName = "/xxx/eee/yyy";
                using (var ms = new MemoryStream())
                {
                    sftp.DownloadFile(remoteFileName, ms);
                }

                sftp.Disconnect();
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

                    testInfo.UploadResult = sftp.BeginUploadFile(testInfo.UploadedFile, 
                        remoteFile,
                        null, 
                        null) as SftpUploadAsyncResult;


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
                        var sftpResult = testInfo.UploadResult;

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
                    testInfo.DownloadResult = sftp.BeginDownloadFile(remoteFile, 
                        testInfo.DownloadedFile,
                        null, 
                        null) as SftpDownloadAsyncResult;

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
                        var sftpResult = testInfo.DownloadResult;

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

                    if (!(testInfo.UploadResult.UploadedBytes > 0 && testInfo.DownloadResult.DownloadedBytes > 0 && testInfo.DownloadResult.DownloadedBytes == testInfo.UploadResult.UploadedBytes))
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

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test that delegates passed to BeginUploadFile, BeginDownloadFile and BeginListDirectory are actually called.")]
        public void Test_Sftp_Ensure_Async_Delegates_Called_For_BeginFileUpload_BeginFileDownload_BeginListDirectory()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string remoteFileName = Path.GetRandomFileName();
                string localFileName = Path.GetRandomFileName();
                bool uploadDelegateCalled = false;
                bool downloadDelegateCalled = false;
                bool listDirectoryDelegateCalled = false;
                IAsyncResult asyncResult;


                // Test for BeginUploadFile.

                CreateTestFile(localFileName, 1);

                using (var fileStream = File.OpenRead(localFileName))
                {
                    asyncResult = sftp.BeginUploadFile(fileStream, remoteFileName, delegate(IAsyncResult ar)
                    {
                        sftp.EndUploadFile(ar);
                        uploadDelegateCalled = true;

                    }, null);

                    while (!asyncResult.IsCompleted)
                    {
                        Thread.Sleep(500);
                    }
                }

                File.Delete(localFileName);

                Assert.IsTrue(uploadDelegateCalled, "BeginUploadFile");

                // Test for BeginDownloadFile.

                asyncResult = null;
                using (var fileStream = File.OpenWrite(localFileName))
                {
                    asyncResult = sftp.BeginDownloadFile(remoteFileName, fileStream, delegate(IAsyncResult ar)
                    {
                        sftp.EndDownloadFile(ar);
                        downloadDelegateCalled = true;

                    }, null);

                    while (!asyncResult.IsCompleted)
                    {
                        Thread.Sleep(500);
                    }
                }

                File.Delete(localFileName);

                Assert.IsTrue(downloadDelegateCalled, "BeginDownloadFile");

                // Test for BeginListDirectory.

                asyncResult = null;
                asyncResult = sftp.BeginListDirectory(sftp.WorkingDirectory, delegate(IAsyncResult ar)
                {
                    sftp.EndListDirectory(ar);
                    listDirectoryDelegateCalled = true;

                }, null);

                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(500);
                }

                Assert.IsTrue(listDirectoryDelegateCalled, "BeginListDirectory");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginUploadFile")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_BeginUploadFile_StreamIsNull()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.BeginUploadFile(null, "aaaaa", null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginUploadFile")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_BeginUploadFile_FileNameIsWhiteSpace()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.BeginUploadFile(new MemoryStream(), "   ", null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginUploadFile")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_BeginUploadFile_FileNameIsNull()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.BeginUploadFile(new MemoryStream(), null, null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_BeginDownloadFile_StreamIsNull()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.BeginDownloadFile("aaaa", null, null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_BeginDownloadFile_FileNameIsWhiteSpace()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.BeginDownloadFile("   ", new MemoryStream(), null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginDownloadFile")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_BeginDownloadFile_FileNameIsNull()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.BeginDownloadFile(null, new MemoryStream(), null, null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_EndUploadFile_Invalid_Async_Handle()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                var async1 = sftp.BeginListDirectory("/", null, null);
                var filename = Path.GetTempFileName();
                this.CreateTestFile(filename, 100);
                var async2 = sftp.BeginUploadFile(File.OpenRead(filename), "test", null, null);
                sftp.EndUploadFile(async1);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_EndDownloadFile_Invalid_Async_Handle()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                var filename = Path.GetTempFileName();
                this.CreateTestFile(filename, 1);
                sftp.UploadFile(File.OpenRead(filename), "test123");
                var async1 = sftp.BeginListDirectory("/", null, null);
                var async2 = sftp.BeginDownloadFile("test123", new MemoryStream(), null, null);
                sftp.EndDownloadFile(async1);
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
