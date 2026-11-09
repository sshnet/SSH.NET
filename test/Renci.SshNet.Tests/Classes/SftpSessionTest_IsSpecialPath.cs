using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SftpSessionTest_IsSpecialPath
    {
        [TestMethod]
        [DataRow("/", true)]
        [DataRow("/.", true)]
        [DataRow("/test/path/.", true)]
        [DataRow("/..", true)]
        [DataRow("/test/path/..", true)]
        [DataRow("test", true)]
        [DataRow("/test/path", false)]
        public void Test_IsSpecialPath(string fullPath, bool expected)
        {
            var actual = SftpSession.IsSpecialPath(fullPath);

            Assert.AreEqual(expected, actual);
        }
    }
}