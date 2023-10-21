using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Upload_And_Download_1MB_File()
        {
            RemoveAllFiles();

            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var uploadedFileName = Path.GetTempFileName();
                var remoteFileName = Path.GetRandomFileName();

                CreateTestFile(uploadedFileName, 1);

                // Calculate hash value
                var uploadedHash = CalculateMD5(uploadedFileName);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    sftp.UploadFile(file, remoteFileName);
                }

                var downloadedFileName = Path.GetTempFileName();

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
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var uploadedFileName = Path.GetTempFileName();
                var remoteFileName = "/root/1";

                CreateTestFile(uploadedFileName, 1);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    sftp.UploadFile(file, remoteFileName);
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

            RemoveAllFiles();

            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.OperationTimeout = TimeSpan.FromMinutes(1);
                sftp.Connect();

                var testInfoList = new Dictionary<string, TestInfo>();

                for (var i = 0; i < maxFiles; i++)
                {
                    var testInfo = new TestInfo
                        {
                            UploadedFileName = Path.GetTempFileName(),
                            DownloadedFileName = Path.GetTempFileName(),
                            RemoteFileName = Path.GetRandomFileName()
                        };

                    CreateTestFile(testInfo.UploadedFileName, maxSize);

                    // Calculate hash value
                    testInfo.UploadedHash = CalculateMD5(testInfo.UploadedFileName);

                    testInfoList.Add(testInfo.RemoteFileName, testInfo);
                }

                var uploadWaitHandles = new List<WaitHandle>();

                // Start file uploads
                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];
                    testInfo.UploadedFile = File.OpenRead(testInfo.UploadedFileName);

                    testInfo.UploadResult = sftp.BeginUploadFile(testInfo.UploadedFile,
                                                                 remoteFile,
                                                                 asyncCallback: null,
                                                                 state: null) as SftpUploadAsyncResult;

                    uploadWaitHandles.Add(testInfo.UploadResult.AsyncWaitHandle);
                }

                // Wait for upload to finish
                var uploadCompleted = false;
                while (!uploadCompleted)
                {
                    // Assume upload completed
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

                // End file uploads
                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];

                    sftp.EndUploadFile(testInfo.UploadResult);
                    testInfo.UploadedFile.Dispose();
                }

                // Start file downloads
                var downloadWaitHandles = new List<WaitHandle>();

                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];
                    testInfo.DownloadedFile = File.OpenWrite(testInfo.DownloadedFileName);
                    testInfo.DownloadResult = sftp.BeginDownloadFile(remoteFile,
                                                                     testInfo.DownloadedFile,
                                                                     asyncCallback: null,
                                                                     state: null) as SftpDownloadAsyncResult;

                    downloadWaitHandles.Add(testInfo.DownloadResult.AsyncWaitHandle);
                }

                // Wait for download to finish
                var downloadCompleted = false;
                while (!downloadCompleted)
                {
                    // Assume download completed
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

                // End file downloads
                foreach (var remoteFile in testInfoList.Keys)
                {
                    var testInfo = testInfoList[remoteFile];

                    sftp.EndDownloadFile(testInfo.DownloadResult);

                    testInfo.DownloadedFile.Dispose();

                    testInfo.DownloadedHash = CalculateMD5(testInfo.DownloadedFileName);

                    Console.WriteLine(remoteFile);
                    Console.WriteLine("UploadedBytes: "+ testInfo.UploadResult.UploadedBytes);
                    Console.WriteLine("DownloadedBytes: " + testInfo.DownloadResult.DownloadedBytes);
                    Console.WriteLine("UploadedHash: " + testInfo.UploadedHash);
                    Console.WriteLine("DownloadedHash: " + testInfo.DownloadedHash);
                    if (!(testInfo.UploadResult.UploadedBytes > 0 && testInfo.DownloadResult.DownloadedBytes > 0 && testInfo.DownloadResult.DownloadedBytes == testInfo.UploadResult.UploadedBytes))
                    {
                        uploadDownloadSizeOk = false;
                    }

                    if (!testInfo.DownloadedHash.Equals(testInfo.UploadedHash, StringComparison.Ordinal))
                    {
                        hashMatches = false;
                    }
                }

                // Clean up after test
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

        // TODO: Split this test into multiple tests
        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test that delegates passed to BeginUploadFile, BeginDownloadFile and BeginListDirectory are actually called.")]
        public void Test_Sftp_Ensure_Async_Delegates_Called_For_BeginFileUpload_BeginFileDownload_BeginListDirectory()
        {
            RemoveAllFiles();

            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();

                var remoteFileName = Path.GetRandomFileName();
                var localFileName = Path.GetRandomFileName();
                var uploadDelegateCalled = false;
                var downloadDelegateCalled = false;
                var listDirectoryDelegateCalled = false;
                IAsyncResult asyncResult;

                /*
                 * Test for BeginUploadFile.
                 */

                CreateTestFile(localFileName, 1);

                using (var fileStream = File.OpenRead(localFileName))
                {
                    asyncResult = sftp.BeginUploadFile(fileStream,
                                                       remoteFileName,
                                                       ar =>
                                                            {
                                                               sftp.EndUploadFile(ar);
                                                               uploadDelegateCalled = true;
                                                            },
                                                       state: null);

                    while (!asyncResult.IsCompleted)
                    {
                        Thread.Sleep(500);
                    }
                }

                File.Delete(localFileName);

                Assert.IsTrue(uploadDelegateCalled, "BeginUploadFile");

                /*
                 * Test for BeginDownloadFile.
                 */

                asyncResult = null;
                using (var fileStream = File.OpenWrite(localFileName))
                {
                    asyncResult = sftp.BeginDownloadFile(remoteFileName,
                                                         fileStream,
                                                         ar =>
                                                            {
                                                                sftp.EndDownloadFile(ar);
                                                                downloadDelegateCalled = true;
                                                            },
                                                         state: null);

                    while (!asyncResult.IsCompleted)
                    {
                        Thread.Sleep(500);
                    }
                }

                File.Delete(localFileName);

                Assert.IsTrue(downloadDelegateCalled, "BeginDownloadFile");

                /*
                 * Test for BeginListDirectory.
                 */

                asyncResult = null;
                asyncResult = sftp.BeginListDirectory(sftp.WorkingDirectory,
                                                      ar =>
                                                        {
                                                            _ = sftp.EndListDirectory(ar);
                                                            listDirectoryDelegateCalled = true;
                                                        },
                                                      state: null);

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
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                _ = sftp.BeginUploadFile(input: null, "aaaaa", asyncCallback: null, state: null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginUploadFile")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_BeginUploadFile_FileNameIsWhiteSpace()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                _ = sftp.BeginUploadFile(new MemoryStream(), "   ", asyncCallback: null, state: null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to BeginUploadFile")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_BeginUploadFile_FileNameIsNull()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                _ = sftp.BeginUploadFile(new MemoryStream(), path: null, asyncCallback: null, state: null);
                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_EndUploadFile_Invalid_Async_Handle()
        {
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                var async1 = sftp.BeginListDirectory("/", asyncCallback: null, state: null);
                var filename = Path.GetTempFileName();
                CreateTestFile(filename, 100);

                using (var fs = File.OpenRead(filename))
                {
                    var async2 = sftp.BeginUploadFile(fs, "test", asyncCallback: null, state: null);
                    sftp.EndUploadFile(async1);
                }
            }
        }
    }
}
