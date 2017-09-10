using System;
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

        [TestMethod]
        public void Load_Data()
        {
            const uint one = 123456u;
            const uint two = 456789u;

            var sshDataStream = new SshDataStream(8);
            sshDataStream.Write(one);
            sshDataStream.Write(two);

            var sshData = sshDataStream.ToArray();

            var request = new RequestSshData();
            request.Load(sshData);

            Assert.AreEqual(one, request.ValueOne);
            Assert.AreEqual(two, request.ValueTwo);
        }

        [TestMethod]
        public void Load_Data_ShouldThrowArgumentNullExceptionWhenDataIsNull()
        {
            const byte[] sshData = null;
            var request = new RequestSshData();

            try
            {
                request.Load(sshData);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("data", ex.ParamName);
            }
        }

        [TestMethod]
        public void Load_DataAndOffsetAndCount()
        {
            const uint one = 123456u;
            const uint two = 456789u;

            var sshDataStream = new SshDataStream(11);
            sshDataStream.WriteByte(0x05);
            sshDataStream.WriteByte(0x07);
            sshDataStream.WriteByte(0x0f);
            sshDataStream.Write(one);
            sshDataStream.Write(two);

            var sshData = sshDataStream.ToArray();

            var request = new RequestSshData();
            request.Load(sshData, 3, sshData.Length - 3);

            Assert.AreEqual(one, request.ValueOne);
            Assert.AreEqual(two, request.ValueTwo);
        }

        private class BoolSshData : SshData
        {
            private readonly bool _value;

            public BoolSshData(bool value)
            {
                _value = value;
            }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
                Write(_value);
            }
        }

        private class RequestSshData : SshData
        {
            private uint _valueOne;
            private uint _valueTwo;

            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // ValueOne
                    capacity += 4; // ValueTwo
                    return capacity;
                }
            }

            public uint ValueOne
            {
                get { return _valueOne; }
                set { _valueOne = value; }
            }

            public uint ValueTwo
            {
                get { return _valueTwo; }
                set { _valueTwo = value; }
            }

            protected override void LoadData()
            {
                _valueOne = ReadUInt32();
                _valueTwo = ReadUInt32();
            }

            protected override void SaveData()
            {
                Write(ValueOne);
                Write(ValueTwo);
            }
        }
    }
}
