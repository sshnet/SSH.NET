using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Properties;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.ScpClientTests
{
    [TestClass]
    public class UploadDownloadTest
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

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_File_20_Parallel_Upload_Download()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                var uploadFilenames = new string[20];
                for (int i = 0; i < uploadFilenames.Length; i++)
                {
                    uploadFilenames[i] = Path.GetTempFileName();
                    this.CreateTestFile(uploadFilenames[i], 1);
                }

                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Upload(new FileInfo(filename), Path.GetFileName(filename));
                    });

                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Download(Path.GetFileName(filename), new FileInfo(string.Format("{0}.down", filename)));
                    });

                var result = from file in uploadFilenames
                             where
                                 CalculateMD5(file) == CalculateMD5(string.Format("{0}.down", file))
                             select file;


                scp.Disconnect();

                Assert.IsTrue(result.Count() == uploadFilenames.Length);
            }
        }

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_File_Upload_Download_Events()
        {
            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                var uploadFilenames = new string[10];

                for (int i = 0; i < uploadFilenames.Length; i++)
                {
                    uploadFilenames[i] = Path.GetTempFileName();
                    this.CreateTestFile(uploadFilenames[i], 1);
                }

                var uploadedFiles = uploadFilenames.ToDictionary((filename) => Path.GetFileName(filename), (filename) => 0L);
                var downloadedFiles = uploadFilenames.ToDictionary((filename) => string.Format("{0}.down", Path.GetFileName(filename)), (filename) => 0L);

                scp.Uploading += delegate(object sender, ScpUploadEventArgs e)
                {
                    uploadedFiles[e.Filename] = e.Uploaded;
                };

                scp.Downloading += delegate(object sender, ScpDownloadEventArgs e)
                {
                    downloadedFiles[string.Format("{0}.down", e.Filename)] = e.Downloaded;
                };


                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Upload(new FileInfo(filename), Path.GetFileName(filename));
                    });

                Parallel.ForEach(uploadFilenames,
                    (filename) =>
                    {
                        scp.Download(Path.GetFileName(filename), new FileInfo(string.Format("{0}.down", filename)));
                    });

                var result = from uf in uploadedFiles
                             from df in downloadedFiles
                             where
                                 string.Format("{0}.down", uf.Key) == df.Key
                                 && uf.Value == df.Value
                             select uf;


                scp.Disconnect();

                Assert.IsTrue(result.Count() == uploadFilenames.Length && uploadFilenames.Length == uploadedFiles.Count && uploadedFiles.Count == downloadedFiles.Count);
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
