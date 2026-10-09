using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileTest
    {
        [TestMethod]
        public void LongName_FileFromADirectoryListing_IsTheLineSentByTheServer()
        {
            const string longName = "drwxr-xr-x    3 owner    group        1024 Oct  9 14:31 dir";
            var attributes = SftpFileAttributesBuilder.Empty;
            attributes.LongName = longName;

            var target = new SftpFile(new Mock<ISftpSession>(MockBehavior.Strict).Object, "/home/owner/dir", attributes);

            Assert.AreEqual(longName, target.LongName);
        }

        [TestMethod]
        public void LongName_WithoutLongNameFromTheServer_IsNull()
        {
            var target = new SftpFile(new Mock<ISftpSession>(MockBehavior.Strict).Object, "/home/owner/file.txt", SftpFileAttributesBuilder.Empty);

            Assert.IsNull(target.LongName);
        }
    }
}
