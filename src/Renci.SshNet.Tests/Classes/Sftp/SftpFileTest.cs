using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
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
        [TestCategory("integration")]
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
        [TestCategory("integration")]
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
        [TestCategory("integration")]
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
        [TestCategory("integration")]
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
        [TestCategory("integration")]
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
        [TestCategory("integration")]
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

        /// <summary>
        ///A test for Delete
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DeleteTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            target.Delete();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for MoveTo
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void MoveToTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            string destFileName = string.Empty; // TODO: Initialize to an appropriate value
            target.MoveTo(destFileName);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetPermissions
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SetPermissionsTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            short mode = 0; // TODO: Initialize to an appropriate value
            target.SetPermissions(mode);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ToStringTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UpdateStatus
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void UpdateStatusTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            target.UpdateStatus();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GroupCanExecute
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GroupCanExecuteTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.GroupCanExecute = expected;
            actual = target.GroupCanExecute;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GroupCanRead
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GroupCanReadTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.GroupCanRead = expected;
            actual = target.GroupCanRead;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GroupCanWrite
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GroupCanWriteTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.GroupCanWrite = expected;
            actual = target.GroupCanWrite;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GroupId
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GroupIdTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.GroupId = expected;
            actual = target.GroupId;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsBlockDevice
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void IsBlockDeviceTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsBlockDevice;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsCharacterDevice
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void IsCharacterDeviceTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsCharacterDevice;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void IsDirectoryTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsDirectory;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsNamedPipe
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void IsNamedPipeTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsNamedPipe;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsRegularFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void IsRegularFileTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsRegularFile;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsSocket
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void IsSocketTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsSocket;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsSymbolicLink
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void IsSymbolicLinkTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsSymbolicLink;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LastAccessTime
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void LastAccessTimeTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            target.LastAccessTime = expected;
            actual = target.LastAccessTime;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LastAccessTimeUtc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void LastAccessTimeUtcTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            target.LastAccessTimeUtc = expected;
            actual = target.LastAccessTimeUtc;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LastWriteTime
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void LastWriteTimeTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            target.LastWriteTime = expected;
            actual = target.LastWriteTime;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LastWriteTimeUtc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void LastWriteTimeUtcTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            target.LastWriteTimeUtc = expected;
            actual = target.LastWriteTimeUtc;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Length
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void LengthTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            long actual;
            actual = target.Length;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OthersCanExecute
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OthersCanExecuteTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.OthersCanExecute = expected;
            actual = target.OthersCanExecute;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OthersCanRead
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OthersCanReadTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.OthersCanRead = expected;
            actual = target.OthersCanRead;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OthersCanWrite
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OthersCanWriteTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.OthersCanWrite = expected;
            actual = target.OthersCanWrite;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OwnerCanExecute
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OwnerCanExecuteTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.OwnerCanExecute = expected;
            actual = target.OwnerCanExecute;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OwnerCanRead
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OwnerCanReadTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.OwnerCanRead = expected;
            actual = target.OwnerCanRead;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OwnerCanWrite
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OwnerCanWriteTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.OwnerCanWrite = expected;
            actual = target.OwnerCanWrite;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UserId
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void UserIdTest()
        {
            SftpSession sftpSession = null; // TODO: Initialize to an appropriate value
            string fullName = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes attributes = null; // TODO: Initialize to an appropriate value
            SftpFile target = new SftpFile(sftpSession, fullName, attributes); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.UserId = expected;
            actual = target.UserId;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

    }
}