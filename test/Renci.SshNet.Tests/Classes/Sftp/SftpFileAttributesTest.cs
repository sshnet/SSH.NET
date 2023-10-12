using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    ///This is a test class for SftpFileAttributesTest and is intended
    ///to contain all SftpFileAttributesTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholders only
    public class SftpFileAttributesTest : TestBase
    {
        /// <summary>
        ///A test for SetPermissions
        ///</summary>
        [TestMethod()]
        public void SetPermissionsTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
            short mode = 0; // TODO: Initialize to an appropriate value
            target.SetPermissions(mode);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GroupCanExecute
        ///</summary>
        [TestMethod()]
        public void GroupCanExecuteTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void GroupCanReadTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void GroupCanWriteTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void GroupIdTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.GroupId = expected;
            actual = target.GroupId;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LastAccessTime
        ///</summary>
        [TestMethod()]
        public void LastAccessTimeTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            target.LastAccessTime = expected;
            actual = target.LastAccessTime;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LastWriteTime
        ///</summary>
        [TestMethod()]
        public void LastWriteTimeTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
            DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
            DateTime actual;
            target.LastWriteTime = expected;
            actual = target.LastWriteTime;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OthersCanExecute
        ///</summary>
        [TestMethod()]
        public void OthersCanExecuteTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void OthersCanReadTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void OthersCanWriteTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void OwnerCanExecuteTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void OwnerCanReadTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void OwnerCanWriteTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.OwnerCanWrite = expected;
            actual = target.OwnerCanWrite;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Size
        ///</summary>
        [TestMethod()]
        public void SizeTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            target.Size = expected;
            actual = target.Size;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UserId
        ///</summary>
        [TestMethod()]
        public void UserIdTest()
        {
            SftpFileAttributes target = SftpFileAttributes.Empty; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.UserId = expected;
            actual = target.UserId;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
