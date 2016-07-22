using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    ///This is a test class for SftpFileSytemInformationTest and is intended
    ///to contain all SftpFileSytemInformationTest Unit Tests
    ///</summary>
    [TestClass]
    public class SftpFileSystemInformationTest : TestBase
    {
        /// <summary>
        ///A test for IsReadOnly
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void IsReadOnlyTest()
        {
            ulong bsize = 0; // TODO: Initialize to an appropriate value
            ulong frsize = 0; // TODO: Initialize to an appropriate value
            ulong blocks = 0; // TODO: Initialize to an appropriate value
            ulong bfree = 0; // TODO: Initialize to an appropriate value
            ulong bavail = 0; // TODO: Initialize to an appropriate value
            ulong files = 0; // TODO: Initialize to an appropriate value
            ulong ffree = 0; // TODO: Initialize to an appropriate value
            ulong favail = 0; // TODO: Initialize to an appropriate value
            ulong sid = 0; // TODO: Initialize to an appropriate value
            ulong flag = 0; // TODO: Initialize to an appropriate value
            ulong namemax = 0; // TODO: Initialize to an appropriate value
            SftpFileSytemInformation target = new SftpFileSytemInformation(bsize, frsize, blocks, bfree, bavail, files, ffree, favail, sid, flag, namemax); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsReadOnly;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SupportsSetUid
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void SupportsSetUidTest()
        {
            ulong bsize = 0; // TODO: Initialize to an appropriate value
            ulong frsize = 0; // TODO: Initialize to an appropriate value
            ulong blocks = 0; // TODO: Initialize to an appropriate value
            ulong bfree = 0; // TODO: Initialize to an appropriate value
            ulong bavail = 0; // TODO: Initialize to an appropriate value
            ulong files = 0; // TODO: Initialize to an appropriate value
            ulong ffree = 0; // TODO: Initialize to an appropriate value
            ulong favail = 0; // TODO: Initialize to an appropriate value
            ulong sid = 0; // TODO: Initialize to an appropriate value
            ulong flag = 0; // TODO: Initialize to an appropriate value
            ulong namemax = 0; // TODO: Initialize to an appropriate value
            SftpFileSytemInformation target = new SftpFileSytemInformation(bsize, frsize, blocks, bfree, bavail, files, ffree, favail, sid, flag, namemax); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.SupportsSetUid;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
