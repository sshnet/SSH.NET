using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
#if FEATURE_TPL
using System.Threading.Tasks;
#endif // FEATURE_TPL

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    [TestClass]
    public partial class ScpClientTest : TestBase
    {
        private Random _random;

        [TestInitialize]
        public void SetUp()
        {
            _random = new Random();
        }

        [TestMethod]
        public void Ctor_ConnectionInfo_Null()
        {
            const ConnectionInfo connectionInfo = null;

            try
            {
                new ScpClient(connectionInfo);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("connectionInfo", ex.ParamName);
            }
        }

        [TestMethod]
        public void Ctor_ConnectionInfo_NotNull()
        {
            var connectionInfo = new ConnectionInfo("HOST", "USER", new PasswordAuthenticationMethod("USER", "PWD"));

            var client = new ScpClient(connectionInfo);
            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.AreSame(connectionInfo, client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            Assert.IsNull(client.Session);
        }

        [TestMethod]
        public void Ctor_HostAndPortAndUsernameAndPassword()
        {
            var host = _random.Next().ToString();
            var port = _random.Next(1, 100);
            var userName = _random.Next().ToString();
            var password = _random.Next().ToString();

            var client = new ScpClient(host, port, userName, password);
            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            Assert.IsNull(client.Session);

            var passwordConnectionInfo = client.ConnectionInfo as PasswordConnectionInfo;
            Assert.IsNotNull(passwordConnectionInfo);
            Assert.AreEqual(host, passwordConnectionInfo.Host);
            Assert.AreEqual(port, passwordConnectionInfo.Port);
            Assert.AreSame(userName, passwordConnectionInfo.Username);
            Assert.IsNotNull(passwordConnectionInfo.AuthenticationMethods);
            Assert.AreEqual(1, passwordConnectionInfo.AuthenticationMethods.Count);

            var passwordAuthentication = passwordConnectionInfo.AuthenticationMethods[0] as PasswordAuthenticationMethod;
            Assert.IsNotNull(passwordAuthentication);
            Assert.AreEqual(userName, passwordAuthentication.Username);
            Assert.IsTrue(Encoding.UTF8.GetBytes(password).IsEqualTo(passwordAuthentication.Password));
        }

        [TestMethod]
        public void Ctor_HostAndUsernameAndPassword()
        {
            var host = _random.Next().ToString();
            var userName = _random.Next().ToString();
            var password = _random.Next().ToString();

            var client = new ScpClient(host, userName, password);
            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            Assert.IsNull(client.Session);

            var passwordConnectionInfo = client.ConnectionInfo as PasswordConnectionInfo;
            Assert.IsNotNull(passwordConnectionInfo);
            Assert.AreEqual(host, passwordConnectionInfo.Host);
            Assert.AreEqual(22, passwordConnectionInfo.Port);
            Assert.AreSame(userName, passwordConnectionInfo.Username);
            Assert.IsNotNull(passwordConnectionInfo.AuthenticationMethods);
            Assert.AreEqual(1, passwordConnectionInfo.AuthenticationMethods.Count);

            var passwordAuthentication = passwordConnectionInfo.AuthenticationMethods[0] as PasswordAuthenticationMethod;
            Assert.IsNotNull(passwordAuthentication);
            Assert.AreEqual(userName, passwordAuthentication.Username);
            Assert.IsTrue(Encoding.UTF8.GetBytes(password).IsEqualTo(passwordAuthentication.Password));
        }

        [TestMethod]
        public void Ctor_HostAndPortAndUsernameAndPrivateKeys()
        {
            var host = _random.Next().ToString();
            var port = _random.Next(1, 100);
            var userName = _random.Next().ToString();
            var privateKeys = new[] {GetRsaKey(), GetDsaKey()};

            var client = new ScpClient(host, port, userName, privateKeys);
            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            Assert.IsNull(client.Session);

            var privateKeyConnectionInfo = client.ConnectionInfo as PrivateKeyConnectionInfo;
            Assert.IsNotNull(privateKeyConnectionInfo);
            Assert.AreEqual(host, privateKeyConnectionInfo.Host);
            Assert.AreEqual(port, privateKeyConnectionInfo.Port);
            Assert.AreSame(userName, privateKeyConnectionInfo.Username);
            Assert.IsNotNull(privateKeyConnectionInfo.AuthenticationMethods);
            Assert.AreEqual(1, privateKeyConnectionInfo.AuthenticationMethods.Count);

            var privateKeyAuthentication = privateKeyConnectionInfo.AuthenticationMethods[0] as PrivateKeyAuthenticationMethod;
            Assert.IsNotNull(privateKeyAuthentication);
            Assert.AreEqual(userName, privateKeyAuthentication.Username);
            Assert.IsNotNull(privateKeyAuthentication.KeyFiles);
            Assert.AreEqual(privateKeys.Length, privateKeyAuthentication.KeyFiles.Count);
            Assert.IsTrue(privateKeyAuthentication.KeyFiles.Contains(privateKeys[0]));
            Assert.IsTrue(privateKeyAuthentication.KeyFiles.Contains(privateKeys[1]));
        }

        [TestMethod]
        public void Ctor_HostAndUsernameAndPrivateKeys()
        {
            var host = _random.Next().ToString();
            var userName = _random.Next().ToString();
            var privateKeys = new[] { GetRsaKey(), GetDsaKey() };

            var client = new ScpClient(host, userName, privateKeys);
            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            Assert.IsNull(client.Session);

            var privateKeyConnectionInfo = client.ConnectionInfo as PrivateKeyConnectionInfo;
            Assert.IsNotNull(privateKeyConnectionInfo);
            Assert.AreEqual(host, privateKeyConnectionInfo.Host);
            Assert.AreEqual(22, privateKeyConnectionInfo.Port);
            Assert.AreSame(userName, privateKeyConnectionInfo.Username);
            Assert.IsNotNull(privateKeyConnectionInfo.AuthenticationMethods);
            Assert.AreEqual(1, privateKeyConnectionInfo.AuthenticationMethods.Count);

            var privateKeyAuthentication = privateKeyConnectionInfo.AuthenticationMethods[0] as PrivateKeyAuthenticationMethod;
            Assert.IsNotNull(privateKeyAuthentication);
            Assert.AreEqual(userName, privateKeyAuthentication.Username);
            Assert.IsNotNull(privateKeyAuthentication.KeyFiles);
            Assert.AreEqual(privateKeys.Length, privateKeyAuthentication.KeyFiles.Count);
            Assert.IsTrue(privateKeyAuthentication.KeyFiles.Contains(privateKeys[0]));
            Assert.IsTrue(privateKeyAuthentication.KeyFiles.Contains(privateKeys[1]));
        }

        [TestMethod]
        public void RemotePathTransformation_Value_NotNull()
        {
            var client = new ScpClient("HOST", 22, "USER", "PWD");

            Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
            Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
        }

        [TestMethod]
        public void RemotePathTransformation_Value_Null()
        {
            var client = new ScpClient("HOST", 22, "USER", "PWD")
            {
                RemotePathTransformation = RemotePathTransformation.ShellQuote
            };

            try
            {
                client.RemotePathTransformation = null;
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("value", ex.ParamName);
            }

            Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
        }

        [TestMethod]
        [TestCategory("Scp")]
        [TestCategory("integration")]
        public void Test_Scp_File_Upload_Download()
        {
            RemoveAllFiles();

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
        [TestCategory("integration")]
        public void Test_Scp_Stream_Upload_Download()
        {
            RemoveAllFiles();

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
        [TestCategory("integration")]
        public void Test_Scp_10MB_File_Upload_Download()
        {
            RemoveAllFiles();

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
        [TestCategory("integration")]
        public void Test_Scp_10MB_Stream_Upload_Download()
        {
            RemoveAllFiles();

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
        [TestCategory("integration")]
        public void Test_Scp_Directory_Upload_Download()
        {
            RemoveAllFiles();

            using (var scp = new ScpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                scp.Connect();

                var uploadDirectory =
                    Directory.CreateDirectory(string.Format("{0}\\{1}", Path.GetTempPath(), Path.GetRandomFileName()));
                for (int i = 0; i < 3; i++)
                {
                    var subfolder =
                        Directory.CreateDirectory(string.Format(@"{0}\folder_{1}", uploadDirectory.FullName, i));
                    for (int j = 0; j < 5; j++)
                    {
                        this.CreateTestFile(string.Format(@"{0}\file_{1}", subfolder.FullName, j), 1);
                    }
                    this.CreateTestFile(string.Format(@"{0}\file_{1}", uploadDirectory.FullName, i), 1);
                }

                scp.Upload(uploadDirectory, "uploaded_dir");

                var downloadDirectory =
                    Directory.CreateDirectory(string.Format("{0}\\{1}", Path.GetTempPath(), Path.GetRandomFileName()));

                scp.Download("uploaded_dir", downloadDirectory);

                var uploadedFiles = uploadDirectory.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                var downloadFiles = downloadDirectory.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

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
        }

        /// <summary>
        ///A test for OperationTimeout
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DownloadTest2()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            ScpClient target = new ScpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            Stream destination = null; // TODO: Initialize to an appropriate value
            target.Download(filename, destination);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

#if FEATURE_TPL
        [TestMethod]
        [TestCategory("Scp")]
        [TestCategory("integration")]
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
        [TestCategory("integration")]
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

                scp.Uploading += delegate (object sender, ScpUploadEventArgs e)
                {
                    uploadedFiles[e.Filename] = e.Uploaded;
                };

                scp.Downloading += delegate (object sender, ScpDownloadEventArgs e)
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
#endif // FEATURE_TPL

        protected static string CalculateMD5(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open))
            {
                var md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                var sb = new StringBuilder();
                for (var i = 0; i < retVal.Length; i++)
                {
                    sb.Append(i.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static void RemoveAllFiles()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.RunCommand("rm -rf *");
                client.Disconnect();
            }
        }

        private PrivateKeyFile GetRsaKey()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                return new PrivateKeyFile(stream);
            }
        }

        private PrivateKeyFile GetDsaKey()
        {
            using (var stream = GetData("Key.SSH2.DSA.txt"))
            {
                return new PrivateKeyFile(stream);
            }
        }
    }
}