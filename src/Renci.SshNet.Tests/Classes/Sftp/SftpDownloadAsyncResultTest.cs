using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
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
