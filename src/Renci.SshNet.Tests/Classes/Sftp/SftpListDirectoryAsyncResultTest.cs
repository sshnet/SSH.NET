using Renci.SshNet.Sftp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for SftpListDirectoryAsyncResultTest and is intended
    ///to contain all SftpListDirectoryAsyncResultTest Unit Tests
    ///</summary>
    [TestClass]
    public class SftpListDirectoryAsyncResultTest : TestBase
    {
        /// <summary>
        ///A test for SftpListDirectoryAsyncResult Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void SftpListDirectoryAsyncResultConstructorTest()
        {
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            SftpListDirectoryAsyncResult target = new SftpListDirectoryAsyncResult(asyncCallback, state);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
