using Renci.SshNet.Sftp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for SftpDownloadAsyncResultTest and is intended
    ///to contain all SftpDownloadAsyncResultTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SftpDownloadAsyncResultTest : TestBase
    {
        /// <summary>
        ///A test for SftpDownloadAsyncResult Constructor
        ///</summary>
        [TestMethod()]
        public void SftpDownloadAsyncResultConstructorTest()
        {
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            SftpDownloadAsyncResult target = new SftpDownloadAsyncResult(asyncCallback, state);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
