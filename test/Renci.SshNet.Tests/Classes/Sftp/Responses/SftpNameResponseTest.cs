using System.Collections.Generic;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp.Responses
{
    [TestClass]
    public class SftpNameResponseTest
    {
        private const string LongName = "-rw-r--r--    1 owner    group        1024 Oct  9 14:31 file.txt";

        [TestMethod]
        public void Load_ProtocolVersion3_KeepsTheLongNameOfEachFile()
        {
            var attributesBytes = SftpFileAttributesBuilder.Empty.GetBytes();
            var sshDataStream = new SshDataStream(0);
            sshDataStream.Write(1U); // response id
            sshDataStream.Write(2U); // count
            sshDataStream.Write("file.txt", Encoding.UTF8);
            sshDataStream.Write(LongName, Encoding.UTF8);
            sshDataStream.Write(attributesBytes);
            sshDataStream.Write("other", Encoding.UTF8);
            sshDataStream.Write(string.Empty, Encoding.UTF8);
            sshDataStream.Write(attributesBytes);

            var target = new SftpNameResponse(3, Encoding.UTF8);
            target.Load(sshDataStream.ToArray());

            Assert.AreEqual(2U, target.Count);
            Assert.AreEqual("file.txt", target.Files[0].Key);
            Assert.AreEqual(LongName, target.Files[0].Value.LongName);
            Assert.AreEqual("other", target.Files[1].Key);
            Assert.AreEqual(string.Empty, target.Files[1].Value.LongName);
        }

        [TestMethod]
        public void Load_ProtocolVersion4_HasNoLongName()
        {
            var attributesBytes = SftpFileAttributesBuilder.Empty.GetBytes();
            var sshDataStream = new SshDataStream(0);
            sshDataStream.Write(1U); // response id
            sshDataStream.Write(1U); // count
            sshDataStream.Write("file.txt", Encoding.UTF8);
            sshDataStream.Write(attributesBytes);

            var target = new SftpNameResponse(4, Encoding.UTF8);
            target.Load(sshDataStream.ToArray());

            Assert.AreEqual("file.txt", target.Files[0].Key);
            Assert.IsNull(target.Files[0].Value.LongName);
        }

        [TestMethod]
        public void GetBytes_ProtocolVersion3_WritesTheLongName()
        {
            var attributes = SftpFileAttributesBuilder.Empty;
            attributes.LongName = LongName;
            var target = new SftpNameResponse(3, Encoding.UTF8)
            {
                ResponseId = 7,
                Files = [new KeyValuePair<string, SftpFileAttributes>("file.txt", attributes)],
            };

            var bytes = target.GetBytes();

            // 4 bytes for the length of the SFTP data, 1 byte for the SFTP message type
            var loaded = new SftpNameResponse(3, Encoding.UTF8);
            loaded.Load(bytes, 5, bytes.Length - 5);
            Assert.AreEqual(7U, loaded.ResponseId);
            Assert.AreEqual("file.txt", loaded.Files[0].Key);
            Assert.AreEqual(LongName, loaded.Files[0].Value.LongName);
        }
    }
}
