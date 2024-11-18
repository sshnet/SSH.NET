using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    [TestClass]
    public partial class ScpClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_File_Upload_Download()
        {
            RemoveAllFiles();

            using (var scp = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                scp.Connect();

                var uploadedFileName = Path.GetTempFileName();
                var downloadedFileName = Path.GetTempFileName();

                CreateTestFile(uploadedFileName, 1);

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
            RemoveAllFiles();

            using (var scp = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                scp.Connect();

                var uploadedFileName = Path.GetTempFileName();
                var downloadedFileName = Path.GetTempFileName();

                CreateTestFile(uploadedFileName, 1);

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
            RemoveAllFiles();

            using (var scp = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                scp.Connect();

                var uploadedFileName = Path.GetTempFileName();
                var downloadedFileName = Path.GetTempFileName();

                CreateTestFile(uploadedFileName, 10);

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
            RemoveAllFiles();

            using (var scp = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                scp.Connect();

                var uploadedFileName = Path.GetTempFileName();
                var downloadedFileName = Path.GetTempFileName();

                CreateTestFile(uploadedFileName, 10);

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
            RemoveAllFiles();
            using (var sftp = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                sftp.Connect();
                sftp.CreateDirectory("uploaded_dir");
            }

            using (var scp = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                scp.Connect();

                var uploadDirectory =
                    Directory.CreateDirectory(string.Format("{0}\\{1}", Path.GetTempPath(), Path.GetRandomFileName()));
                for (var i = 0; i < 3; i++)
                {
                    var subfolder = Directory.CreateDirectory(string.Format(@"{0}\folder_{1}", uploadDirectory.FullName, i));

                    for (var j = 0; j < 5; j++)
                    {
                        CreateTestFile(string.Format(@"{0}\file_{1}", subfolder.FullName, j), 1);
                    }

                    CreateTestFile(string.Format(@"{0}\file_{1}", uploadDirectory.FullName, i), 1);
                }

                scp.Upload(uploadDirectory, "uploaded_dir");

                var downloadDirectory =
                    Directory.CreateDirectory(string.Format("{0}\\{1}", Path.GetTempPath(), Path.GetRandomFileName()));

                scp.Download("uploaded_dir", downloadDirectory);

                var uploadedFiles = uploadDirectory.GetFiles("*.*", SearchOption.AllDirectories);
                var downloadFiles = downloadDirectory.GetFiles("*.*", SearchOption.AllDirectories);

                var result = from f1 in uploadedFiles
                             from f2 in downloadFiles
                             where
                                 f1.FullName.Substring(uploadDirectory.FullName.Length) ==
                                 f2.FullName.Substring(downloadDirectory.FullName.Length)
                                 && CalculateMD5(f1.FullName) == CalculateMD5(f2.FullName)
                             select f1;

                var counter = result.Count();

                scp.Disconnect();

                Assert.IsTrue(counter == uploadedFiles.Length && uploadedFiles.Length == downloadFiles.Length);
            }
            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_File_20_Parallel_Upload_Download()
        {
            using (var scp = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                scp.Connect();

                var uploadFilenames = new string[20];
                for (var i = 0; i < uploadFilenames.Length; i++)
                {
                    uploadFilenames[i] = Path.GetTempFileName();
                    CreateTestFile(uploadFilenames[i], 1);
                }

                _ = Parallel.ForEach(uploadFilenames,
                                     filename =>
                                        {
                                            scp.Upload(new FileInfo(filename), Path.GetFileName(filename));
                                        });
                _ = Parallel.ForEach(uploadFilenames,
                                     filename =>
                                        {
                                            scp.Download(Path.GetFileName(filename), new FileInfo(string.Format("{0}.down", filename)));
                                        });

                var result = from file in uploadFilenames
                             where CalculateMD5(file) == CalculateMD5(string.Format("{0}.down", file))
                             select file;

                scp.Disconnect();

                Assert.IsTrue(result.Count() == uploadFilenames.Length);
            }
        }

        [TestMethod]
        [TestCategory("Scp")]
        public void Test_Scp_File_Upload_Download_Events()
        {
            using (var scp = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                scp.Connect();

                var uploadFilenames = new string[10];

                for (var i = 0; i < uploadFilenames.Length; i++)
                {
                    uploadFilenames[i] = Path.GetTempFileName();
                    CreateTestFile(uploadFilenames[i], 1);
                }

                var uploadedFiles = uploadFilenames.ToDictionary(Path.GetFileName, (filename) => 0L);
                var downloadedFiles = uploadFilenames.ToDictionary((filename) => string.Format("{0}.down", Path.GetFileName(filename)), (filename) => 0L);

                scp.Uploading += delegate (object sender, ScpUploadEventArgs e)
                {
                    uploadedFiles[e.Filename] = e.Uploaded;
                };

                scp.Downloading += delegate (object sender, ScpDownloadEventArgs e)
                {
                    downloadedFiles[string.Format("{0}.down", e.Filename)] = e.Downloaded;
                };

                _ = Parallel.ForEach(uploadFilenames,
                                     filename =>
                                        {
                                            scp.Upload(new FileInfo(filename), Path.GetFileName(filename));
                                        });
                _ = Parallel.ForEach(uploadFilenames,
                                     filename =>
                                        {
                                            scp.Download(Path.GetFileName(filename), new FileInfo(string.Format("{0}.down", filename)));
                                        });

                var result = from uf in uploadedFiles
                             from df in downloadedFiles
                             where string.Format("{0}.down", uf.Key) == df.Key && uf.Value == df.Value
                             select uf;

                scp.Disconnect();

                Assert.IsTrue(result.Count() == uploadFilenames.Length && uploadFilenames.Length == uploadedFiles.Count && uploadedFiles.Count == downloadedFiles.Count);
            }
        }

        protected static string CalculateMD5(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open))
            {
#if NET7_0_OR_GREATER
                var hash = MD5.HashData(file);
#else
#if NET6_0
                var md5 = MD5.Create();
#else
                MD5 md5 = new MD5CryptoServiceProvider();
#endif // NET6_0
                var hash = md5.ComputeHash(file);
#endif // NET7_0_OR_GREATER

                file.Close();

                var sb = new StringBuilder();

                for (var i = 0; i < hash.Length; i++)
                {
                    _ = sb.Append(i.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private void RemoveAllFiles()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                _ = client.RunCommand("rm -rf *");
                client.Disconnect();
            }
        }
    }
}
