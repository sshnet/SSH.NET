using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes
{

    [TestClass]
    public class SftpSessionTest_ExtractPartialFullPath
    {
        [TestMethod]
        [DataRow("/a/b/c", "/a/b")]
        [DataRow("/c", "/")]
        public void Test_ExtractPartialFullPath(string fullPath, string expected)
        {
            var pathParts = fullPath.Split('/');
            var actual = SftpSession.ExtractPartialFullPath(pathParts);
            Assert.AreEqual(expected, actual);
        }
    }
}