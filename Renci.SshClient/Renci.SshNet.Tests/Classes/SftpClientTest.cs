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
        /// <summary>
        ///A test for SftpClient Constructor
        ///</summary>
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        public void SftpClientConstructorTest4()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChangePermissions
        ///</summary>
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        public void EndListDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            IAsyncResult asyncResult = null; // TODO: Initialize to an appropriate value
            IEnumerable<SftpFile> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<SftpFile> actual;
            actual = target.EndListDirectory(asyncResult);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EndDownloadFile
        ///</summary>
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        public void GetTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            SftpFile expected = null; // TODO: Initialize to an appropriate value
            SftpFile actual;
            actual = target.Get(path);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Exists
        ///</summary>
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        public void ListDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            Action<int> listCallback = null; // TODO: Initialize to an appropriate value
            IEnumerable<SftpFile> expected = null; // TODO: Initialize to an appropriate value
            IEnumerable<SftpFile> actual;
            actual = target.ListDirectory(path, listCallback);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Open
        ///</summary>
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        public void SetLastAccessTimeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastAccessTime = new DateTime(); // TODO: Initialize to an appropriate value
            target.SetLastAccessTime(path, lastAccessTime);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLastAccessTimeUtc
        ///</summary>
        [TestMethod()]
        public void SetLastAccessTimeUtcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastAccessTimeUtc = new DateTime(); // TODO: Initialize to an appropriate value
            target.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLastWriteTime
        ///</summary>
        [TestMethod()]
        public void SetLastWriteTimeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastWriteTime = new DateTime(); // TODO: Initialize to an appropriate value
            target.SetLastWriteTime(path, lastWriteTime);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLastWriteTimeUtc
        ///</summary>
        [TestMethod()]
        public void SetLastWriteTimeUtcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string path = string.Empty; // TODO: Initialize to an appropriate value
            DateTime lastWriteTimeUtc = new DateTime(); // TODO: Initialize to an appropriate value
            target.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SymbolicLink
        ///</summary>
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        public void WorkingDirectoryTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SftpClient target = new SftpClient(connectionInfo); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.WorkingDirectory;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }


        protected override void OnInit()
        {
            base.OnInit();
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.RunCommand("rm -rf *");
                client.Disconnect();
            }
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