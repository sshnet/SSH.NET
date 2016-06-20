using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Diagnostics;
using System.Linq;

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
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_ListDirectory_Permission_Denied()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.ListDirectory("/root");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
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
        [TestCategory("integration")]
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
        [TestCategory("integration")]
        public void Test_Sftp_ListDirectory_Empty()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.ListDirectory(string.Empty);

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
        [TestCategory("integration")]
        [Description("Test passing null to ListDirectory.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_ListDirectory_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.ListDirectory(null);

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
        [TestCategory("integration")]
        public void Test_Sftp_ListDirectory_HugeDirectory()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                //  Create 10000 directory items
                for (int i = 0; i < 10000; i++)
                {
                    sftp.CreateDirectory(string.Format("test_{0}", i));
                    Debug.WriteLine("Created " + i);
                }

                var files = sftp.ListDirectory(".");

                //  Ensure that directory has at least 10000 items
                Assert.IsTrue(files.Count() > 10000);

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
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

                sftp.ChangeDirectory("/home/tester/test1/test1_1");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester/test1/test1_1");

                sftp.ChangeDirectory("/home/tester/test1/test1_1/../test1_2");

                Assert.AreEqual(sftp.WorkingDirectory, "/home/tester/test1/test1_2");

                sftp.ChangeDirectory("../../");

                sftp.DeleteDirectory("test1/test1_1");
                sftp.DeleteDirectory("test1/test1_2");
                sftp.DeleteDirectory("test1/test1_3");
                sftp.DeleteDirectory("test1");

                sftp.Disconnect();
            }

            RemoveAllFiles();
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [Description("Test passing null to ChangeDirectory.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_ChangeDirectory_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                sftp.ChangeDirectory(null);

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [Description("Test calling EndListDirectory method more then once.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Sftp_Call_EndListDirectory_Twice()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();
                var ar = sftp.BeginListDirectory("/", null, null);
                var result = sftp.EndListDirectory(ar);
                var result1 = sftp.EndListDirectory(ar);
            }
        }
    }
}