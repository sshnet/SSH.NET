using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.IO;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    /// Represents SFTP file information
    /// </summary>
    [TestClass]
    public class SftpFileTest : TestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_Root_Directory()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                var directory = sftp.Get("/");

                Assert.AreEqual("/", directory.FullName);
                Assert.IsTrue(directory.IsDirectory);
                Assert.IsFalse(directory.IsRegularFile);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Get_Invalid_Directory()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.Get("/xyz");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_File()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.UploadFile(new MemoryStream(), "abc.txt");

                var file = sftp.Get("abc.txt");

                Assert.AreEqual("/home/tester/abc.txt", file.FullName);
                Assert.IsTrue(file.IsRegularFile);
                Assert.IsFalse(file.IsDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [Description("Test passing null to Get.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Get_File_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var file = sftp.Get(null);

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Get_International_File()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.UploadFile(new MemoryStream(), "test-üöä-");

                var file = sftp.Get("test-üöä-");

                Assert.AreEqual("/home/tester/test-üöä-", file.FullName);
                Assert.IsTrue(file.IsRegularFile);
                Assert.IsFalse(file.IsDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_SftpFile_MoveTo()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                string uploadedFileName = Path.GetTempFileName();
                string remoteFileName = Path.GetRandomFileName();
                string newFileName = Path.GetRandomFileName();

                this.CreateTestFile(uploadedFileName, 1);

                using (var file = File.OpenRead(uploadedFileName))
                {
                    sftp.UploadFile(file, remoteFileName);
                }

                var sftpFile = sftp.Get(remoteFileName);

                sftpFile.MoveTo(newFileName);

                Assert.AreEqual(newFileName, sftpFile.Name);

                sftp.Disconnect();
            }
        }
    }
}