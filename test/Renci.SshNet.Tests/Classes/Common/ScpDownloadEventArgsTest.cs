using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for ScpDownloadEventArgsTest and is intended
    ///to contain all ScpDownloadEventArgsTest Unit Tests
    ///</summary>
    [TestClass]
    public class ScpDownloadEventArgsTest : TestBase
    {
        /// <summary>
        ///A test for ScpDownloadEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void ScpDownloadEventArgsConstructorTest()
        {
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            long size = 0; // TODO: Initialize to an appropriate value
            long downloaded = 0; // TODO: Initialize to an appropriate value
            ScpDownloadEventArgs target = new ScpDownloadEventArgs(filename, size, downloaded);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
