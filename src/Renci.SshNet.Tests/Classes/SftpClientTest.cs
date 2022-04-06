using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    [TestClass]
    public partial class SftpClientTest : TestBase
    {
        private Random _random;

        [TestInitialize]
        public void SetUp()
        {
            _random = new Random();
        }

        [TestMethod]
        public void OperationTimeout_Default()
        {
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);

            var actual = target.OperationTimeout;

            Assert.AreEqual(TimeSpan.FromMilliseconds(-1), actual);
        }

        [TestMethod]
        public void OperationTimeout_InsideLimits()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(_random.Next(0, int.MaxValue - 1));
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_LowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-1);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_UpperLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_LessThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-2);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);

            try
            {
                target.OperationTimeout = operationTimeout;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
#if NETFRAMEWORK
                Assert.AreEqual("The timeout must represent a value between -1 and Int32.MaxValue, inclusive." + Environment.NewLine + "Parameter name: " + ex.ParamName, ex.Message);
#else
                Assert.AreEqual("The timeout must represent a value between -1 and Int32.MaxValue, inclusive. (Parameter '" + ex.ParamName + "')", ex.Message);
#endif
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void OperationTimeout_GreaterThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue).Add(TimeSpan.FromMilliseconds(1));
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);

            try
            {
                target.OperationTimeout = operationTimeout;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
#if NETFRAMEWORK
                Assert.AreEqual("The timeout must represent a value between -1 and Int32.MaxValue, inclusive." + Environment.NewLine + "Parameter name: " + ex.ParamName, ex.Message);
#else
                Assert.AreEqual("The timeout must represent a value between -1 and Int32.MaxValue, inclusive. (Parameter '" + ex.ParamName + "')", ex.Message);
#endif
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void OperationTimeout_Disposed()
        {
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new SftpClient(connectionInfo);
            target.Dispose();

            // getter
            try
            {
                var actual = target.OperationTimeout;
                Assert.Fail("Should have failed, but returned: " + actual);
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(typeof(SftpClient).FullName, ex.ObjectName);
            }

            // setter
            try
            {
                target.OperationTimeout = TimeSpan.FromMilliseconds(5);
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(typeof(SftpClient).FullName, ex.ObjectName);
            }
        }

        /// <summary>
        ///A test for SftpClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SftpClientConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(host, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SftpClientConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(host, port, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SftpClientConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SftpClientConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SftpClientConstructorTest4()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChangePermissions
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ChangePermissionsTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            short mode = 0; // TODO: Initialize to an appropriate value
            target.ChangePermissions(path, mode);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ChangeDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ChangeDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            target.ChangeDirectory(path);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for BeginUploadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BeginUploadFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            Action<ulong> uploadCallback = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginUploadFile(input, path, asyncCallback, state, uploadCallback);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BeginUploadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BeginUploadFileTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            bool canOverride = false; // TODO: Initialize to an appropriate value
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            Action<ulong> uploadCallback = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginUploadFile(input, path, canOverride, asyncCallback, state, uploadCallback);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BeginSynchronizeDirectories
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BeginSynchronizeDirectoriesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string sourcePath = string.Empty; // TODO: Initialize to an appropriate value
            string destinationPath = string.Empty; // TODO: Initialize to an appropriate value
            string searchPattern = string.Empty; // TODO: Initialize to an appropriate value
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginSynchronizeDirectories(sourcePath, destinationPath, searchPattern, asyncCallback, state);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BeginListDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BeginListDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            Action<int> listCallback = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginListDirectory(path, asyncCallback, state, listCallback);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BeginDownloadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BeginDownloadFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            Action<ulong> downloadCallback = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginDownloadFile(path, output, asyncCallback, state, downloadCallback);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AppendText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AppendTextTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            StreamWriter expected = null; // TODO: Initialize to an appropriate value
            StreamWriter actual;
            actual = target.AppendText(path, encoding);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AppendText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AppendTextTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            StreamWriter expected = null; // TODO: Initialize to an appropriate value
            StreamWriter actual;
            actual = target.AppendText(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AppendAllText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AppendAllTextTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string contents = string.Empty; // TODO: Initialize to an appropriate value
            target.AppendAllText(path, contents);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for AppendAllText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AppendAllTextTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string contents = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            target.AppendAllText(path, contents, encoding);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for AppendAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AppendAllLinesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<string> contents = null; // TODO: Initialize to an appropriate value
            target.AppendAllLines(path, contents);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for AppendAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AppendAllLinesTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<string> contents = null; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            target.AppendAllLines(path, contents, encoding);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CreateText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateTextTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            StreamWriter expected = null; // TODO: Initialize to an appropriate value
            StreamWriter actual;
            actual = target.CreateText(path, encoding);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateTextTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            StreamWriter expected = null; // TODO: Initialize to an appropriate value
            StreamWriter actual;
            actual = target.CreateText(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            target.CreateDirectory(path);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            SftpFileStream expected = null; // TODO: Initialize to an appropriate value
            SftpFileStream actual;
            actual = target.Create(path, bufferSize);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileStream expected = null; // TODO: Initialize to an appropriate value
            SftpFileStream actual;
            actual = target.Create(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EndSynchronizeDirectories
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EndSynchronizeDirectoriesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            IAsyncResult asyncResult = null; // TODO: Initialize to an appropriate value
            IEnumerable<FileInfo> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<FileInfo> actual;
            actual = target.EndSynchronizeDirectories(asyncResult);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EndListDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EndListDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            IAsyncResult asyncResult = null; // TODO: Initialize to an appropriate value
            IEnumerable<ISftpFile> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<ISftpFile> actual;
            actual = target.EndListDirectory(asyncResult);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EndDownloadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EndDownloadFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            IAsyncResult asyncResult = null; // TODO: Initialize to an appropriate value
            target.EndDownloadFile(asyncResult);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DownloadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DownloadFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Action<ulong> downloadCallback = null; // TODO: Initialize to an appropriate value
            target.DownloadFile(path, output, downloadCallback);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DeleteFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DeleteFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            target.DeleteFile(path);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DeleteDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DeleteDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            target.DeleteDirectory(path);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Delete
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DeleteTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            target.Delete(path);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GetLastAccessTimeUtc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GetLastAccessTimeUtcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = target.GetLastAccessTimeUtc(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetLastAccessTime
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GetLastAccessTimeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = target.GetLastAccessTime(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetAttributes
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GetAttributesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes expected = null; // TODO: Initialize to an appropriate value
            SftpFileAttributes actual;
            actual = target.GetAttributes(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GetTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            ISftpFile expected = null; // TODO: Initialize to an appropriate value
            ISftpFile actual;
            actual = target.Get(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Exists
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ExistsTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Exists(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EndUploadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EndUploadFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            IAsyncResult asyncResult = null; // TODO: Initialize to an appropriate value
            target.EndUploadFile(asyncResult);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GetLastWriteTimeUtc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GetLastWriteTimeUtcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = target.GetLastWriteTimeUtc(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetLastWriteTime
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GetLastWriteTimeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = target.GetLastWriteTime(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetStatus
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void GetStatusTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileSytemInformation expected = null; // TODO: Initialize to an appropriate value
            SftpFileSytemInformation actual;
            actual = target.GetStatus(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ListDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ListDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Action<int> listCallback = null; // TODO: Initialize to an appropriate value
            IEnumerable<ISftpFile> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<ISftpFile> actual;
            actual = target.ListDirectory(path, listCallback);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Open
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OpenTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            FileMode mode = new FileMode(); // TODO: Initialize to an appropriate value
            SftpFileStream expected = null; // TODO: Initialize to an appropriate value
            SftpFileStream actual;
            actual = target.Open(path, mode);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Open
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OpenTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            FileMode mode = new FileMode(); // TODO: Initialize to an appropriate value
            FileAccess access = new FileAccess(); // TODO: Initialize to an appropriate value
            SftpFileStream expected = null; // TODO: Initialize to an appropriate value
            SftpFileStream actual;
            actual = target.Open(path, mode, access);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OpenRead
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OpenReadTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileStream expected = null; // TODO: Initialize to an appropriate value
            SftpFileStream actual;
            actual = target.OpenRead(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OpenText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OpenTextTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            StreamReader expected = null; // TODO: Initialize to an appropriate value
            StreamReader actual;
            actual = target.OpenText(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OpenWrite
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OpenWriteTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileStream expected = null; // TODO: Initialize to an appropriate value
            SftpFileStream actual;
            actual = target.OpenWrite(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadAllBytes
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ReadAllBytesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.ReadAllBytes(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ReadAllLinesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            string[] expected = null; // TODO: Initialize to an appropriate value
            string[] actual;
            actual = target.ReadAllLines(path, encoding);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ReadAllLinesTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string[] expected = null; // TODO: Initialize to an appropriate value
            string[] actual;
            actual = target.ReadAllLines(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadAllText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ReadAllTextTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ReadAllText(path, encoding);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadAllText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ReadAllTextTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ReadAllText(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ReadLinesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<string> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<string> actual;
            actual = target.ReadLines(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReadLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ReadLinesTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            IEnumerable<string> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<string> actual;
            actual = target.ReadLines(path, encoding);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for RenameFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void RenameFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string oldPath = string.Empty; // TODO: Initialize to an appropriate value
            string newPath = string.Empty; // TODO: Initialize to an appropriate value
            target.RenameFile(oldPath, newPath);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RenameFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void RenameFileTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string oldPath = string.Empty; // TODO: Initialize to an appropriate value
            string newPath = string.Empty; // TODO: Initialize to an appropriate value
            bool isPosix = false; // TODO: Initialize to an appropriate value
            target.RenameFile(oldPath, newPath, isPosix);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetAttributes
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SetAttributesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            SftpFileAttributes fileAttributes = null; // TODO: Initialize to an appropriate value
            target.SetAttributes(path, fileAttributes);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLastAccessTime
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SetLastAccessTimeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastAccessTime = new DateTime(); // TODO: Initialize to an appropriate value
#pragma warning disable CS0618 // Type or member is obsolete
            target.SetLastAccessTime(path, lastAccessTime);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLastAccessTimeUtc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SetLastAccessTimeUtcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastAccessTimeUtc = new DateTime(); // TODO: Initialize to an appropriate value
#pragma warning disable CS0618 // Type or member is obsolete
            target.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLastWriteTime
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SetLastWriteTimeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastWriteTime = new DateTime(); // TODO: Initialize to an appropriate value
#pragma warning disable CS0618 // Type or member is obsolete
            target.SetLastWriteTime(path, lastWriteTime);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLastWriteTimeUtc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SetLastWriteTimeUtcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastWriteTimeUtc = new DateTime(); // TODO: Initialize to an appropriate value
#pragma warning disable CS0618 // Type or member is obsolete
            target.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SymbolicLink
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SymbolicLinkTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string linkPath = string.Empty; // TODO: Initialize to an appropriate value
            target.SymbolicLink(path, linkPath);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SynchronizeDirectories
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SynchronizeDirectoriesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string sourcePath = string.Empty; // TODO: Initialize to an appropriate value
            string destinationPath = string.Empty; // TODO: Initialize to an appropriate value
            string searchPattern = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<FileInfo> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<FileInfo> actual;
            actual = target.SynchronizeDirectories(sourcePath, destinationPath, searchPattern);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UploadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void UploadFileTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Action<ulong> uploadCallback = null; // TODO: Initialize to an appropriate value
            target.UploadFile(input, path, uploadCallback);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for UploadFile
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void UploadFileTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            bool canOverride = false; // TODO: Initialize to an appropriate value
            Action<ulong> uploadCallback = null; // TODO: Initialize to an appropriate value
            target.UploadFile(input, path, canOverride, uploadCallback);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteAllBytes
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WriteAllBytesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            byte[] bytes = null; // TODO: Initialize to an appropriate value
            target.WriteAllBytes(path, bytes);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WriteAllLinesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<string> contents = null; // TODO: Initialize to an appropriate value
            target.WriteAllLines(path, contents);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WriteAllLinesTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string[] contents = null; // TODO: Initialize to an appropriate value
            target.WriteAllLines(path, contents);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WriteAllLinesTest2()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<string> contents = null; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            target.WriteAllLines(path, contents, encoding);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteAllLines
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WriteAllLinesTest3()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string[] contents = null; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            target.WriteAllLines(path, contents, encoding);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteAllText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WriteAllTextTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string contents = string.Empty; // TODO: Initialize to an appropriate value
            target.WriteAllText(path, contents);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for WriteAllText
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WriteAllTextTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            string contents = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            target.WriteAllText(path, contents, encoding);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for BufferSize
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BufferSizeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            uint expected = 0; // TODO: Initialize to an appropriate value
            uint actual;
            target.BufferSize = expected;
            actual = target.BufferSize;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OperationTimeout
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OperationTimeoutTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.OperationTimeout = expected;
            actual = target.OperationTimeout;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for WorkingDirectory
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void WorkingDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.WorkingDirectory;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        protected static string CalculateMD5(string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
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

        /// <summary>
        /// Helper class to help with upload and download testing
        /// </summary>
        private class TestInfo
        {
            public string RemoteFileName { get; set; }

            public string UploadedFileName { get; set; }

            public string DownloadedFileName { get; set; }

            //public ulong UploadedBytes { get; set; }

            //public ulong DownloadedBytes { get; set; }

            public FileStream UploadedFile { get; set; }

            public FileStream DownloadedFile { get; set; }

            public string UploadedHash { get; set; }

            public string DownloadedHash { get; set; }

            public SftpUploadAsyncResult UploadResult { get; set; }

            public SftpDownloadAsyncResult DownloadResult { get; set; }
        }
    }
}
