using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for ScpUploadEventArgsTest and is intended
    ///to contain all ScpUploadEventArgsTest Unit Tests
    ///</summary>
    [TestClass]
    public class ScpUploadEventArgsTest : TestBase
    {
        /// <summary>
        ///A test for ScpUploadEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void ScpUploadEventArgsConstructorTest()
        {
            string filename = string.Empty; // TODO: Initialize to an appropriate value
            long size = 0; // TODO: Initialize to an appropriate value
            long uploaded = 0; // TODO: Initialize to an appropriate value
            ScpUploadEventArgs target = new ScpUploadEventArgs(filename, size, uploaded);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
