using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Tests for <see cref="ScpClient.ResolveChild(string, string)"/> and
    /// <see cref="ScpClient.NormalizeDirectory(string)"/> with a root directory as the destination,
    /// which a download test cannot use. Other cases are covered by
    /// <see cref="ScpClientTest_Download_PathAndDirectoryInfo_Names"/>.
    /// </summary>
    [TestClass]
    public class ScpClientTest_ResolveChild_RootDirectory
    {
        [TestMethod]
        public void RootDirectory_ShouldReturnDirectChild()
        {
            var root = ScpClient.NormalizeDirectory(Path.GetPathRoot(Path.GetTempPath()));

            Assert.AreEqual(Path.Combine(root, "file.txt"), ScpClient.ResolveChild(root, "file.txt"));
        }

        [TestMethod]
        [DataRow(".")]
        [DataRow("..")]
        public void RootDirectory_DotOrDotDot_ShouldBeRejected(string name)
        {
            // "." and ".." resolve to the root itself, which has no parent directory.
            var root = ScpClient.NormalizeDirectory(Path.GetPathRoot(Path.GetTempPath()));

            Assert.IsNull(ScpClient.ResolveChild(root, name));
        }
    }
}
