using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    ///This is a test class for SftpUploadAsyncResultTest and is intended
    ///to contain all SftpUploadAsyncResultTest Unit Tests
    ///</summary>
    [TestClass]
    public class SftpUploadAsyncResultTest : TestBase
    {
        /// <summary>
        ///A test for SftpUploadAsyncResult Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void SftpUploadAsyncResultConstructorTest()
        {
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            SftpUploadAsyncResult target = new SftpUploadAsyncResult(asyncCallback, state);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
