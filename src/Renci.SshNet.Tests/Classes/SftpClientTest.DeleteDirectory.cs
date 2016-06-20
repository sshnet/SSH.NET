using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest : TestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_DeleteDirectory_Without_Connecting()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.DeleteDirectory("test");
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_DeleteDirectory_Which_Doesnt_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.DeleteDirectory("abcdef");

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_DeleteDirectory_Which_No_Permissions()
        {
            if (Resources.USERNAME == "root")
                Assert.Fail("Must not run this test as root!");

            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.DeleteDirectory("/usr");

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void Test_Sftp_DeleteDirectory()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.CreateDirectory("abcdef");
                sftp.DeleteDirectory("abcdef");

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [Description("Test passing null to DeleteDirectory.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_DeleteDirectory_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.DeleteDirectory(null);

                sftp.Disconnect();
            }
        }
    }
}