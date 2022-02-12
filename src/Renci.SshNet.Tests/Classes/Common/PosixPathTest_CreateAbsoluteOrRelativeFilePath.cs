using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PosixPathTest_CreateAbsoluteOrRelativeFilePath
    {
        [TestMethod]
        public void Path_Null()
        {
            const string path = null;

            try
            {
                PosixPath.CreateAbsoluteOrRelativeFilePath(path);
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

            try
            {
                PosixPath.CreateAbsoluteOrRelativeFilePath(path);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format("The path is a zero-length string.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
                Assert.AreEqual("path", ex.ParamName);
            }
        }

        [TestMethod]
        public void Path_TrailingForwardSlash()
        {
            var path = "/abc/";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/abc", actual.Directory);
            Assert.IsNull(actual.File);
        }

        [TestMethod]
        public void Path_FileWithoutNoDirectory()
        {
            var path = "abc.log";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual(".", actual.Directory);
            Assert.AreSame(path, actual.File);
        }

        [TestMethod]
        public void Path_FileInRootDirectory()
        {
            var path = "/abc.log";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual.Directory);
            Assert.AreEqual("abc.log", actual.File);
        }

        [TestMethod]
        public void Path_RootDirectoryOnly()
        {
            var path = "/";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual.Directory);
            Assert.IsNull(actual.File);
        }

        [TestMethod]
        public void Path_FileInNonRootDirectory()
        {
            var path = "/home/sshnet/xyz";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/home/sshnet", actual.Directory);
            Assert.AreEqual("xyz", actual.File);
        }

        [TestMethod]
        public void Path_BackslashIsNotConsideredDirectorySeparator()
        {
            var path = "/home\\abc.log";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual.Directory);
            Assert.AreEqual("home\\abc.log", actual.File);
        }

        [TestMethod]
        public void Path_ColonIsNotConsideredPathSeparator()
        {
            var path = "/home:abc.log";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual.Directory);
            Assert.AreEqual("home:abc.log", actual.File);
        }

        [TestMethod]
        public void Path_LeadingWhitespace()
        {
            var path = "  / \tabc";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("  ", actual.Directory);
            Assert.AreEqual(" \tabc", actual.File);
        }

        [TestMethod]
        public void Path_TrailingWhitespace()
        {
            var path = "/abc \t ";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/", actual.Directory);
            Assert.AreEqual("abc \t ", actual.File);
        }

        [TestMethod]
        public void Path_OnlyWhitespace()
        {
            var path = " ";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual(".", actual.Directory);
            Assert.AreSame(path, actual.File);
        }

        [TestMethod]
        public void Path_FileNameOnlyWhitespace()
        {
            var path = "/home/\t ";

            var actual = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            Assert.IsNotNull(actual);
            Assert.AreEqual("/home", actual.Directory);
            Assert.AreEqual("\t ", actual.File);
        }
    }
}
