using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshClient.Common;
using Renci.SshClient.Tests.Properties;
using System.Diagnostics;

namespace Renci.SshClient.Tests.SftpClientTests
{
    [TestClass]
    public class ListDirectoryTest
    {
        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_ListDirectory_Without_Connecting()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                var files = sftp.ListDirectory(".");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.AbsolutePath);
                }
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshPermissionDeniedException))]
        public void Test_Sftp_ListDirectory_Permission_Denied()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.ListDirectory("/etc/audit");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.AbsolutePath);
                }

                sftp.Disconnect();
            }
        }


        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshFileNotFoundException))]
        public void Test_Sftp_ListDirectory_Not_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.ListDirectory("/asdfgh");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.AbsolutePath);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ListDirectory_Current()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.ListDirectory(".");

                Assert.IsTrue(files.Count() > 0);

                foreach (var file in files)
                {
                    Debug.WriteLine(file.AbsolutePath);
                }

                sftp.Disconnect();
            }
        }



        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_ListDirectory_HugeDirectory()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                //  Create 30000 directory items
                for (int i = 0; i < 30000; i++)
                {
                    sftp.CreateDirectory(string.Format("test_{0}", i));
                }

                var files = sftp.ListDirectory(".");

                //  Ensure that directory has at least 30000 items
                Assert.IsTrue(files.Count() > 30000);

                //  Delete 10000 directory items
                for (int i = 0; i < 30000; i++)
                {
                    sftp.DeleteDirectory(string.Format("test_{0}", i));
                }

                sftp.Disconnect();
            }
        }


    }
}
