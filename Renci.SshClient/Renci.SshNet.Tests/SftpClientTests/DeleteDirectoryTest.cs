using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.SftpClientTests
{
    [TestClass]
    public class DeleteDirectoryTest
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
        [ExpectedException(typeof(SshFileNotFoundException))]
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
        [ExpectedException(typeof(SshPermissionDeniedException))]
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
		[Description("Test passing null to DeleteDirectory.")]		
		[ExpectedException(typeof(ArgumentNullException))]
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
