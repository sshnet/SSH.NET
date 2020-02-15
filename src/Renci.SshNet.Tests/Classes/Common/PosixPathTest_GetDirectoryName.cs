using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PosixPathTest_GetDirectoryName
    {
        [TestMethod]
        public void Path_Null()
        {
            const string path = null;

            try
            {
                PosixPath.GetDirectoryName(path);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("path", ex.ParamName);
            }
        }

        [TestMethod]
        public void Path_Empty()
        {
            var path = string.Empty;

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual(".", actual);
        }

        [TestMethod]
        public void Path_TrailingForwardSlash()
        {
            var path = "/abc/";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/abc", actual);
        }

        [TestMethod]
        public void Path_FileWithoutNoDirectory()
        {
            var path = "abc.log";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual(".", actual);
        }

        [TestMethod]
        public void Path_FileInRootDirectory()
        {
            var path = "/abc.log";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual);
        }

        [TestMethod]
        public void Path_RootDirectoryOnly()
        {
            var path = "/";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual);
        }

        [TestMethod]
        public void Path_FileInNonRootDirectory()
        {
            var path = "/home/sshnet/xyz";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/home/sshnet", actual);
        }

        [TestMethod]
        public void Path_BackslashIsNotConsideredDirectorySeparator()
        {
            var path = "/home\\abc.log";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual);
        }

        [TestMethod]
        public void Path_ColonIsNotConsideredPathSeparator()
        {
            var path = "/home:abc.log";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual);
        }

        [TestMethod]
        public void Path_LeadingWhitespace()
        {
            var path = "  / \tabc";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("  ", actual);
        }

        [TestMethod]
        public void Path_TrailingWhitespace()
        {
            var path = "/abc \t ";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual);
        }

        [TestMethod]
        public void Path_OnlyWhitespace()
        {
            var path = " ";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual(".", actual);
        }

        [TestMethod]
        public void Path_FileNameOnlyWhitespace()
        {
            var path = "/home/\t ";

            var actual = PosixPath.GetDirectoryName(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/home", actual);
        }
    }
}
