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
            var sshData = new BoolSshData(false);
            
            var bytes = sshData.GetBytes();

            Assert.AreEqual((byte) 0, bytes[0]);
        }

        [TestMethod]
        public void Write_Boolean_True()
        {
            var sshData = new BoolSshData(true);

            var bytes = sshData.GetBytes();

            Assert.AreEqual((byte) 1, bytes[0]);
        }

        private class BoolSshData : SshData
        {
            private readonly bool _value;

            public BoolSshData(bool value)
            {
                _value = value;
            }

            public new bool IsEndOfData
            {
                get { return base.IsEndOfData; }
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
                Write(_value);
            }
        }
    }
}
