using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PackTest
    {
        [TestMethod]
        public void BigEndianToUInt16()
        {
            Assert.AreEqual(0, Pack.BigEndianToUInt16(new byte[] {0, 0}));
            Assert.AreEqual(1, Pack.BigEndianToUInt16(new byte[] {0, 1}));
            Assert.AreEqual(256, Pack.BigEndianToUInt16(new byte[] {1, 0}));
            Assert.AreEqual(257, Pack.BigEndianToUInt16(new byte[] {1, 1}));
            Assert.AreEqual(1025, Pack.BigEndianToUInt16(new byte[] {4, 1}));
            Assert.AreEqual(ushort.MaxValue, Pack.BigEndianToUInt16(new byte[] {255, 255}));
        }

        [TestMethod]
        public void BigEndianToUInt32()
        {
            Assert.AreEqual(0U, Pack.BigEndianToUInt32(new byte[] {0, 0, 0, 0}));
            Assert.AreEqual(1U, Pack.BigEndianToUInt32(new byte[] {0, 0, 0, 1}));
            Assert.AreEqual(256U, Pack.BigEndianToUInt32(new byte[] {0, 0, 1, 0}));
            Assert.AreEqual(257U, Pack.BigEndianToUInt32(new byte[] {0, 0, 1, 1}));
            Assert.AreEqual(1025U, Pack.BigEndianToUInt32(new byte[] {0, 0, 4, 1}));
            Assert.AreEqual(65536U, Pack.BigEndianToUInt32(new byte[] {0, 1, 0, 0}));
            Assert.AreEqual(133124U, Pack.BigEndianToUInt32(new byte[] {0, 2, 8, 4}));
            Assert.AreEqual(16777216U, Pack.BigEndianToUInt32(new byte[] {1, 0, 0, 0}));
            Assert.AreEqual(uint.MaxValue, Pack.BigEndianToUInt32(new byte[] {255, 255, 255, 255}));
        }

        [TestMethod]
        public void BigEndianToUInt64()
        {
            Assert.AreEqual(0UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 0, 0, 0}));
            Assert.AreEqual(1UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 0, 0, 1}));
            Assert.AreEqual(256UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 0, 1, 0}));
            Assert.AreEqual(257UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 0, 1, 1}));
            Assert.AreEqual(65536UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 1, 0, 0}));
            Assert.AreEqual(133124UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 2, 8, 4}));
            Assert.AreEqual(16777216UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 0, 1, 0, 0, 0}));
            Assert.AreEqual(4294967296UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 0, 1, 0, 0, 0, 0}));
            Assert.AreEqual(1099511627776UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 1, 0, 0, 0, 0, 0}));
            Assert.AreEqual(1099511892096UL, Pack.BigEndianToUInt64(new byte[] {0, 0, 1, 0, 0, 4, 8, 128}));
            Assert.AreEqual(1099511627776UL * 256, Pack.BigEndianToUInt64(new byte[] {0, 1, 0, 0, 0, 0, 0, 0}));
            Assert.AreEqual(1099511627776UL * 256 * 256, Pack.BigEndianToUInt64(new byte[] {1, 0, 0, 0, 0, 0, 0, 0}));
            Assert.AreEqual(ulong.MaxValue, Pack.BigEndianToUInt64(new byte[] {255, 255, 255, 255, 255, 255, 255, 255}));
        }

        [TestMethod]
        public void LittleEndianToUInt16()
        {
            Assert.AreEqual((ushort) 0, Pack.LittleEndianToUInt16(new byte[] {0, 0}));
            Assert.AreEqual((ushort) 1, Pack.LittleEndianToUInt16(new byte[] {1, 0}));
            Assert.AreEqual((ushort) 256, Pack.LittleEndianToUInt16(new byte[] {0, 1}));
            Assert.AreEqual((ushort) 257, Pack.LittleEndianToUInt16(new byte[] {1, 1}));
            Assert.AreEqual((ushort) 1025, Pack.LittleEndianToUInt16(new byte[] {1, 4}));
            Assert.AreEqual(ushort.MaxValue, Pack.LittleEndianToUInt16(new byte[] {255, 255}));
        }

        [TestMethod]
        public void LittleEndianToUInt32()
        {
            Assert.AreEqual(0U, Pack.LittleEndianToUInt32(new byte[] {0, 0, 0, 0}));
            Assert.AreEqual(1U, Pack.LittleEndianToUInt32(new byte[] {1, 0, 0, 0}));
            Assert.AreEqual(256U, Pack.LittleEndianToUInt32(new byte[] {0, 1, 0, 0}));
            Assert.AreEqual(257U, Pack.LittleEndianToUInt32(new byte[] {1, 1, 0, 0}));
            Assert.AreEqual(1025U, Pack.LittleEndianToUInt32(new byte[] {1, 4, 0, 0}));
            Assert.AreEqual(65536U, Pack.LittleEndianToUInt32(new byte[] {0, 0, 1, 0}));
            Assert.AreEqual(133124U, Pack.LittleEndianToUInt32(new byte[] {4, 8, 2, 0}));
            Assert.AreEqual(16777216U, Pack.LittleEndianToUInt32(new byte[] {0, 0, 0, 1}));
            Assert.AreEqual(uint.MaxValue, Pack.LittleEndianToUInt32(new byte[] {255, 255, 255, 255}));
        }

        [TestMethod]
        public void LittleEndianToUInt64()
        {
            Assert.AreEqual(0UL, Pack.LittleEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 0, 0, 0}));
            Assert.AreEqual(1UL, Pack.LittleEndianToUInt64(new byte[] {1, 0, 0, 0, 0, 0, 0, 0}));
            Assert.AreEqual(256UL, Pack.LittleEndianToUInt64(new byte[] {0, 1, 0, 0, 0, 0, 0, 0}));
            Assert.AreEqual(257UL, Pack.LittleEndianToUInt64(new byte[] {1, 1, 0, 0, 0, 0, 0, 0}));
            Assert.AreEqual(65536UL, Pack.LittleEndianToUInt64(new byte[] {0, 0, 1, 0, 0, 0, 0, 0}));
            Assert.AreEqual(133124UL, Pack.LittleEndianToUInt64(new byte[] {4, 8, 2, 0, 0, 0, 0, 0}));
            Assert.AreEqual(16777216UL, Pack.LittleEndianToUInt64(new byte[] {0, 0, 0, 1, 0, 0, 0, 0}));
            Assert.AreEqual(4294967296UL, Pack.LittleEndianToUInt64(new byte[] {0, 0, 0, 0, 1, 0, 0, 0}));
            Assert.AreEqual(1099511627776UL, Pack.LittleEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 1, 0, 0}));
            Assert.AreEqual(1099511892096UL, Pack.LittleEndianToUInt64(new byte[] {128, 8, 4, 0, 0, 1, 0, 0}));
            Assert.AreEqual(1099511627776UL * 256, Pack.LittleEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 0, 1, 0}));
            Assert.AreEqual(1099511627776UL * 256 * 256, Pack.LittleEndianToUInt64(new byte[] {0, 0, 0, 0, 0, 0, 0, 1}));
            Assert.AreEqual(ulong.MaxValue, Pack.LittleEndianToUInt64(new byte[] {255, 255, 255, 255, 255, 255, 255, 255}));
        }

        [TestMethod]
        public void UInt16ToLittleEndian()
        {
            AssertEqual(new byte[] {0, 0}, Pack.UInt16ToLittleEndian(0));
            AssertEqual(new byte[] {1, 0}, Pack.UInt16ToLittleEndian(1));
            AssertEqual(new byte[] {0, 1}, Pack.UInt16ToLittleEndian(256));
            AssertEqual(new byte[] {1, 1}, Pack.UInt16ToLittleEndian(257));
            AssertEqual(new byte[] {1, 4}, Pack.UInt16ToLittleEndian(1025));
            AssertEqual(new byte[] {255, 255}, Pack.UInt16ToLittleEndian(ushort.MaxValue));
        }

        [TestMethod]
        public void UInt32ToLittleEndian()
        {
            AssertEqual(new byte[] {0, 0, 0, 0}, Pack.UInt32ToLittleEndian(0));
            AssertEqual(new byte[] {1, 0, 0, 0}, Pack.UInt32ToLittleEndian(1));
            AssertEqual(new byte[] {0, 1, 0, 0}, Pack.UInt32ToLittleEndian(256));
            AssertEqual(new byte[] {1, 1, 0, 0}, Pack.UInt32ToLittleEndian(257));
            AssertEqual(new byte[] {1, 4, 0, 0}, Pack.UInt32ToLittleEndian(1025));
            AssertEqual(new byte[] {0, 0, 1, 0}, Pack.UInt32ToLittleEndian(65536));
            AssertEqual(new byte[] {4, 8, 2, 0}, Pack.UInt32ToLittleEndian(133124));
            AssertEqual(new byte[] {0, 0, 0, 1}, Pack.UInt32ToLittleEndian(16777216));
            AssertEqual(new byte[] {255, 255, 255, 255}, Pack.UInt32ToLittleEndian(uint.MaxValue));
        }

        [TestMethod]
        public void UInt64ToLittleEndian()
        {
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 0, 0, 0}, Pack.UInt64ToLittleEndian(0UL));
            AssertEqual(new byte[] {1, 0, 0, 0, 0, 0, 0, 0}, Pack.UInt64ToLittleEndian(1UL));
            AssertEqual(new byte[] {0, 1, 0, 0, 0, 0, 0, 0}, Pack.UInt64ToLittleEndian(256UL));
            AssertEqual(new byte[] {1, 1, 0, 0, 0, 0, 0, 0}, Pack.UInt64ToLittleEndian(257UL));
            AssertEqual(new byte[] {0, 0, 1, 0, 0, 0, 0, 0}, Pack.UInt64ToLittleEndian(65536UL));
            AssertEqual(new byte[] {4, 8, 2, 0, 0, 0, 0, 0}, Pack.UInt64ToLittleEndian(133124UL));
            AssertEqual(new byte[] {0, 0, 0, 1, 0, 0, 0, 0}, Pack.UInt64ToLittleEndian(16777216UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 1, 0, 0, 0}, Pack.UInt64ToLittleEndian(4294967296UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 1, 0, 0}, Pack.UInt64ToLittleEndian(1099511627776UL));
            AssertEqual(new byte[] {128, 8, 4, 0, 0, 1, 0, 0}, Pack.UInt64ToLittleEndian(1099511892096UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 0, 1, 0}, Pack.UInt64ToLittleEndian(1099511627776UL * 256));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 0, 0, 1}, Pack.UInt64ToLittleEndian(1099511627776UL * 256 * 256));
            AssertEqual(new byte[] {255, 255, 255, 255, 255, 255, 255, 255}, Pack.UInt64ToLittleEndian(ulong.MaxValue));
        }

        [TestMethod]
        public void UInt16ToBigEndian()
        {
            AssertEqual(new byte[] {0, 0}, Pack.UInt16ToBigEndian(0));
            AssertEqual(new byte[] {0, 1}, Pack.UInt16ToBigEndian(1));
            AssertEqual(new byte[] {1, 0}, Pack.UInt16ToBigEndian(256));
            AssertEqual(new byte[] {1, 1}, Pack.UInt16ToBigEndian(257));
            AssertEqual(new byte[] {4, 1}, Pack.UInt16ToBigEndian(1025));
            AssertEqual(new byte[] {255, 255}, Pack.UInt16ToBigEndian(ushort.MaxValue));
        }

        [TestMethod]
        public void UInt32ToBigEndian()
        {
            AssertEqual(new byte[] {0, 0, 0, 0}, Pack.UInt32ToBigEndian(0));
            AssertEqual(new byte[] {0, 0, 0, 1}, Pack.UInt32ToBigEndian(1));
            AssertEqual(new byte[] {0, 0, 1, 0}, Pack.UInt32ToBigEndian(256));
            AssertEqual(new byte[] {0, 0, 1, 1}, Pack.UInt32ToBigEndian(257));
            AssertEqual(new byte[] {0, 0, 4, 1}, Pack.UInt32ToBigEndian(1025));
            AssertEqual(new byte[] {0, 1, 0, 0}, Pack.UInt32ToBigEndian(65536));
            AssertEqual(new byte[] {0, 2, 8, 4}, Pack.UInt32ToBigEndian(133124));
            AssertEqual(new byte[] {1, 0, 0, 0}, Pack.UInt32ToBigEndian(16777216));
            AssertEqual(new byte[] {255, 255, 255, 255}, Pack.UInt32ToBigEndian(uint.MaxValue));
        }

        [TestMethod]
        public void UInt64ToBigEndian()
        {
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 0, 0, 0}, Pack.UInt64ToBigEndian(0UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 0, 0, 1}, Pack.UInt64ToBigEndian(1UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 0, 1, 0}, Pack.UInt64ToBigEndian(256UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 0, 1, 1}, Pack.UInt64ToBigEndian(257UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 1, 0, 0}, Pack.UInt64ToBigEndian(65536UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 0, 2, 8, 4}, Pack.UInt64ToBigEndian(133124UL));
            AssertEqual(new byte[] {0, 0, 0, 0, 1, 0, 0, 0}, Pack.UInt64ToBigEndian(16777216UL));
            AssertEqual(new byte[] {0, 0, 0, 1, 0, 0, 0, 0}, Pack.UInt64ToBigEndian(4294967296UL));
            AssertEqual(new byte[] {0, 0, 1, 0, 0, 0, 0, 0}, Pack.UInt64ToBigEndian(1099511627776UL));
            AssertEqual(new byte[] {0, 0, 1, 0, 0, 4, 8, 128}, Pack.UInt64ToBigEndian(1099511892096UL));
            AssertEqual(new byte[] {0, 1, 0, 0, 0, 0, 0, 0}, Pack.UInt64ToBigEndian(1099511627776UL * 256));
            AssertEqual(new byte[] {1, 0, 0, 0, 0, 0, 0, 0}, Pack.UInt64ToBigEndian(1099511627776UL * 256 * 256));
            AssertEqual(new byte[] {255, 255, 255, 255, 255, 255, 255, 255}, Pack.UInt64ToBigEndian(ulong.MaxValue));
        }

        private static void AssertEqual(byte[] expected, byte[] actual)
        {
            Assert.IsTrue(expected.IsEqualTo(actual));
        }
    }
}
