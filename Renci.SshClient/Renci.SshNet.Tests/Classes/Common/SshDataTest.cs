using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class SshDataTest
    {
        [TestMethod]
        public void Write_Boolean_False()
        {
            var sshData = new MySshData();
            sshData.Load(new byte[0]);

            sshData.Write(false);
            Assert.AreEqual((byte) 0, sshData.ReadByte());
            Assert.IsTrue(sshData.IsEndOfData);
        }

        [TestMethod]
        public void Write_Boolean_True()
        {
            var sshData = new MySshData();
            sshData.Load(new byte[0]);

            sshData.Write(true);
            Assert.AreEqual((byte) 1, sshData.ReadByte());
            Assert.IsTrue(sshData.IsEndOfData);
        }

        private class MySshData : SshData
        {
            public new void Write(bool data)
            {
                base.Write(data);
            }

            public new byte ReadByte()
            {
                return base.ReadByte();
            }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
            }
        }
    }
}
