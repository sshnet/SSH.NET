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
                    Debug.WriteLine(file.FullName);
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
                    Debug.WriteLine(file.FullName);
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
                    Debug.WriteLine(file.FullName);
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
                    Debug.WriteLine(file.FullName);
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

                //  Create 10000 directory items
                for (int i = 0; i < 10000; i++)
                {
                    sftp.CreateDirectory(string.Format("test_{0}", i));
                }

                var files = sftp.ListDirectory(".");

                //  Ensure that directory has at least 10000 items
                Assert.IsTrue(files.Count() > 10000);

                //  Delete 10000 directory items
                for (int i = 0; i < 10000; i++)
                {
                    sftp.DeleteDirectory(string.Format("test_{0}", i));
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        public void Test_Sftp_Change_Directory()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester");

                sftp.CreateDirectory("test1");

                sftp.ChangeDirectory("test1");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester/test1");

                sftp.CreateDirectory("test1_1");
                sftp.CreateDirectory("test1_2");
                sftp.CreateDirectory("test1_3");

                var files = sftp.ListDirectory(".");

                Assert.IsTrue(files.First().FullName.StartsWith(string.Format("{0}", sftp.WorkingDirectory)));

                sftp.ChangeDirectory("test1_1");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester/test1/test1_1");

                sftp.ChangeDirectory("../test1_2");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester/test1/test1_2");

                sftp.ChangeDirectory("..");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester/test1");

                sftp.ChangeDirectory("..");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester");

                files = sftp.ListDirectory("test1/test1_1");

                Assert.IsTrue(files.First().FullName.StartsWith(string.Format("{0}/test1/test1_1", sftp.WorkingDirectory)));

                sftp.ChangeDirectory("test1/test1_1");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester/test1/test1_1");

                sftp.ChangeDirectory("../../");

                sftp.DeleteDirectory("test1/test1_1");
                sftp.DeleteDirectory("test1/test1_2");
                sftp.DeleteDirectory("test1/test1_3");
                sftp.DeleteDirectory("test1");

                sftp.Disconnect();
            }
        }
    }
}
