using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;
using System;

namespace Renci.SshNet.Tests.Classes {

    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest {

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_Exists_Without_Connecting() {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD)) {
                sftp.Exists("test");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void ExistsTest_DirName() {
            string dirName = DateTime.Now.Ticks.ToString();
            bool result;

            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD)) {
                sftp.Connect();
                sftp.CreateDirectory(dirName);

                result = sftp.Exists(dirName);

                sftp.DeleteDirectory(dirName);
                sftp.Disconnect();
            }

            Assert.IsTrue(result, "Directory could not be found using standard notation (i.e. \"MyDir\").");
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void ExistsTest_DirName_With_ForwardSlashPrefix() {
            string dirName = DateTime.Now.Ticks.ToString();
            bool result;

            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD)) {
                sftp.Connect();
                sftp.CreateDirectory(dirName);

                result = sftp.Exists(string.Concat("/", dirName));

                sftp.DeleteDirectory(dirName);
                sftp.Disconnect();
            }

            Assert.IsTrue(result, "Directory could not be found using forward-slash prefix notation (i.e. \"/MyDir\").");
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void ExistsTest_NestedDirName() {
            string str = DateTime.Now.Ticks.ToString();
            string parentDirName = string.Concat(str, "-Parent");
            string childDirName = string.Concat(str, "-Child");
            bool result;

            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD)) {
                sftp.Connect();
                var wd = sftp.WorkingDirectory;
                sftp.CreateDirectory(parentDirName);
                sftp.ChangeDirectory(parentDirName);
                sftp.CreateDirectory(childDirName);
                sftp.ChangeDirectory(wd);

                result = sftp.Exists(string.Concat(parentDirName, "/", childDirName));

                sftp.ChangeDirectory(parentDirName);
                sftp.DeleteDirectory(childDirName);
                sftp.ChangeDirectory(wd);
                sftp.DeleteDirectory(parentDirName);
                sftp.Disconnect();
            }

            Assert.IsTrue(result, "Nested directory could not be found using standard notation (i.e. \"ParentDir/ChildDir\").");
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void ExistsTest_NestedDirName_With_ForwardSlashPrefix() {
            string str = DateTime.Now.Ticks.ToString();
            string parentDirName = string.Concat(str, "-Parent");
            string childDirName = string.Concat(str, "-Child");
            bool result;

            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD)) {
                sftp.Connect();
                var wd = sftp.WorkingDirectory;
                sftp.CreateDirectory(parentDirName);
                sftp.ChangeDirectory(parentDirName);
                sftp.CreateDirectory(childDirName);
                sftp.ChangeDirectory(wd);

                result = sftp.Exists(string.Concat("/", parentDirName, "/", childDirName));

                sftp.ChangeDirectory(parentDirName);
                sftp.DeleteDirectory(childDirName);
                sftp.ChangeDirectory(wd);
                sftp.DeleteDirectory(parentDirName);
                sftp.Disconnect();
            }

            Assert.IsTrue(result, "Directory could not be found using forward-slash prefix notation (i.e. \"/ParentDir/ChildDir\").");
        }
    }
}