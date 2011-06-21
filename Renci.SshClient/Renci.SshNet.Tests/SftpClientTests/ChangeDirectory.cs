using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Properties;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.SftpClientTests
{
    [TestClass]
    public class ChangeDirectory
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
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_ChangeDirectory_Root_Dont_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/asdasd");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_ChangeDirectory_Root_With_Slash_Dont_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/asdasd/");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_ChangeDirectory_Subfolder_Dont_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/asdasd/sssddds");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_ChangeDirectory_Subfolder_With_Slash_Dont_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/asdasd/sssddds/");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ChangeDirectory_Which_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/usr");
                Assert.AreEqual("/usr", sftp.WorkingDirectory);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ChangeDirectory_Which_Exists_With_Slash()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                sftp.ChangeDirectory("/usr/");
                Assert.AreEqual("/usr", sftp.WorkingDirectory);
            }
        }
    }
}
