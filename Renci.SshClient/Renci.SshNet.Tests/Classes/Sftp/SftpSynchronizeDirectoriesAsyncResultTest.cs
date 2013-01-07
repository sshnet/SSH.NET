using Renci.SshNet.Sftp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for SftpSynchronizeDirectoriesAsyncResultTest and is intended
    ///to contain all SftpSynchronizeDirectoriesAsyncResultTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SftpSynchronizeDirectoriesAsyncResultTest : TestBase
    {
        /// <summary>
        ///A test for SftpSynchronizeDirectoriesAsyncResult Constructor
        ///</summary>
        [TestMethod()]
        public void SftpSynchronizeDirectoriesAsyncResultConstructorTest()
        {
            AsyncCallback asyncCallback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            SftpSynchronizeDirectoriesAsyncResult target = new SftpSynchronizeDirectoriesAsyncResult(asyncCallback, state);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
