using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp.Responses
{
    [TestClass]
    public class SftpNameResponseTest
    {
        [TestMethod]
        public void Load()
        {
            var target = new SftpNameResponse(3, Encoding.UTF8);

            target.Load([
                0, 0, 0, 7, // response id
                0, 0, 0, 1, // count
                0, 0, 0, 1, (byte)'a', // filename
                0, 0, 0, 0, // longname
                0, 0, 0, 0, // attribute flags
            ]);

            Assert.AreEqual(7u, target.ResponseId);
            Assert.HasCount(1, target.Files);
            Assert.AreEqual("a", target.Files[0].Key);
        }

        [TestMethod]
        public void Load_CountExceedsData_Throws()
        {
            var target = new SftpNameResponse(3, Encoding.UTF8);

            var ex = Assert.ThrowsExactly<SshException>(() => target.Load([
                0, 0, 0, 7, // response id
                0xFF, 0xFF, 0xFF, 0xFF, // count
                0, 0, 0, 1, (byte)'a', // filename
                0, 0, 0, 0, // longname
                0, 0, 0, 0, // attribute flags
            ]));

            Assert.AreEqual("Invalid SSH_FXP_NAME count: 4294967295.", ex.Message);
        }
    }
}
