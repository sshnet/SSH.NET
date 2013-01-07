using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    [TestClass]
    public partial class ScpClientTest : TestBase
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

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_File_Upload_Download()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string downloadedFileName = Path.GetTempFileName();

                this.CreateTestFile(uploadedFileName, 1);

                scp.Upload(new FileInfo(uploadedFileName), Path.GetFileName(uploadedFileName));

                scp.Download(Path.GetFileName(uploadedFileName), new FileInfo(downloadedFileName));

                //  Calculate MD5 value
                var uploadedHash = CalculateMD5(uploadedFileName);
                var downloadedHash = CalculateMD5(downloadedFileName);

                File.Delete(uploadedFileName);
                File.Delete(downloadedFileName);

                scp.Disconnect();

                Assert.AreEqual(uploadedHash, downloadedHash);
            }
        }

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_Stream_Upload_Download()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string downloadedFileName = Path.GetTempFileName();

                this.CreateTestFile(uploadedFileName, 1);

                //  Calculate has value
                using (var stream = File.OpenRead(uploadedFileName))
                {
                    scp.Upload(stream, Path.GetFileName(uploadedFileName));
                }

                using (var stream = File.OpenWrite(downloadedFileName))
                {
                    scp.Download(Path.GetFileName(uploadedFileName), stream);
                }

                //  Calculate MD5 value
                var uploadedHash = CalculateMD5(uploadedFileName);
                var downloadedHash = CalculateMD5(downloadedFileName);

                File.Delete(uploadedFileName);
                File.Delete(downloadedFileName);

                scp.Disconnect();

                Assert.AreEqual(uploadedHash, downloadedHash);
            }
        }

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_10MB_File_Upload_Download()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string downloadedFileName = Path.GetTempFileName();

                this.CreateTestFile(uploadedFileName, 10);

                scp.Upload(new FileInfo(uploadedFileName), Path.GetFileName(uploadedFileName));

                scp.Download(Path.GetFileName(uploadedFileName), new FileInfo(downloadedFileName));

                //  Calculate MD5 value
                var uploadedHash = CalculateMD5(uploadedFileName);
                var downloadedHash = CalculateMD5(downloadedFileName);

                File.Delete(uploadedFileName);
                File.Delete(downloadedFileName);

                scp.Disconnect();

                Assert.AreEqual(uploadedHash, downloadedHash);
            }
        }

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_10MB_Stream_Upload_Download()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string downloadedFileName = Path.GetTempFileName();

                this.CreateTestFile(uploadedFileName, 10);

                //  Calculate has value
                using (var stream = File.OpenRead(uploadedFileName))
                {
                    scp.Upload(stream, Path.GetFileName(uploadedFileName));
                }

                using (var stream = File.OpenWrite(downloadedFileName))
                {
                    scp.Download(Path.GetFileName(uploadedFileName), stream);
                }

                //  Calculate MD5 value
                var uploadedHash = CalculateMD5(uploadedFileName);
                var downloadedHash = CalculateMD5(downloadedFileName);

                File.Delete(uploadedFileName);
                File.Delete(downloadedFileName);

                scp.Disconnect();

                Assert.AreEqual(uploadedHash, downloadedHash);
            }
        }

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_Directory_Upload_Download()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                var uploadDirectory = Directory.CreateDirectory(string.Format("{0}\\{1}", Path.GetTempPath(), Path.GetRandomFileName()));
                for (int i = 0; i < 3; i++)
                {
                    var subfolder = Directory.CreateDirectory(string.Format(@"{0}\folder_{1}", uploadDirectory.FullName, i));
                    for (int j = 0; j < 5; j++)
                    {
                        this.CreateTestFile(string.Format(@"{0}\file_{1}", subfolder.FullName, j), 1);
                    }
                    this.CreateTestFile(string.Format(@"{0}\file_{1}", uploadDirectory.FullName, i), 1);
                }

                scp.Upload(uploadDirectory, "uploaded_dir");

                var downloadDirectory = Directory.CreateDirectory(string.Format("{0}\\{1}", Path.GetTempPath(), Path.GetRandomFileName()));

                scp.Download("uploaded_dir", downloadDirectory);

                var uploadedFiles = uploadDirectory.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                var downloadFiles = downloadDirectory.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

                var result = from f1 in uploadedFiles
                             from f2 in downloadFiles
                             where
                                f1.FullName.Substring(uploadDirectory.FullName.Length) == f2.FullName.Substring(downloadDirectory.FullName.Length)
                                && CalculateMD5(f1.FullName) == CalculateMD5(f2.FullName)
                             select f1;

                var counter = result.Count();

                scp.Disconnect();

                Assert.IsTrue(counter == uploadedFiles.Length && uploadedFiles.Length == downloadFiles.Length);
            }
        }

        /// <summary>
        ///A test for OperationTimeout
        ///</summary>
        [TestMethod()]
        public void OperationTimeoutTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.OperationTimeout = expected;
            actual = target.OperationTimeout;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BufferSize
        ///</summary>
        [TestMethod()]
        public void BufferSizeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            uint expected = 0; // TODO: Initialize to an appropriate value
            uint actual;
            target.BufferSize = expected;
            actual = target.BufferSize;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Upload
        ///</summary>
        [TestMethod()]
        public void UploadTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            DirectoryInfo directoryInfo = null; // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            target.Upload(directoryInfo, filename);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Upload
        ///</summary>
        [TestMethod()]
        public void UploadTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            FileInfo fileInfo = null; // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            target.Upload(fileInfo, filename);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Upload
        ///</summary>
        [TestMethod()]
        public void UploadTest2()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream source = null; // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            target.Upload(source, filename);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Download
        ///</summary>
        [TestMethod()]
        public void DownloadTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string directoryName = string.Empty; // TODO: Initialize to an appropriate value
            DirectoryInfo directoryInfo = null; // TODO: Initialize to an appropriate value
            target.Download(directoryName, directoryInfo);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Download
        ///</summary>
        [TestMethod()]
        public void DownloadTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            FileInfo fileInfo = null; // TODO: Initialize to an appropriate value
            target.Download(filename, fileInfo);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Download
        ///</summary>
        [TestMethod()]
        public void DownloadTest2()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            Stream destination = null; // TODO: Initialize to an appropriate value
            target.Download(filename, destination);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ScpClient Constructor
        ///</summary>
        [TestMethod()]
        public void ScpClientConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(host, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ScpClient Constructor
        ///</summary>
        [TestMethod()]
        public void ScpClientConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(host, port, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ScpClient Constructor
        ///</summary>
        [TestMethod()]
        public void ScpClientConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ScpClient Constructor
        ///</summary>
        [TestMethod()]
        public void ScpClientConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ScpClient Constructor
        ///</summary>
        [TestMethod()]
        public void ScpClientConstructorTest4()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo);
            Assert.Inconclusive("TODO: Implement code to verify target");
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
                var md5 = new MD5CryptoServiceProvider();
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
    }
}