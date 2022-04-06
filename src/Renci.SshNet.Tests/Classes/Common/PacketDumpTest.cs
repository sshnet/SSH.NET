using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PacketDumpTest
    {
        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsNull()
        {
            const byte[] data = null;

            try
            {
                PacketDump.Create(data, 0);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("data", ex.ParamName);
            }
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_IndentLevelLessThanZero()
        {
            var data = new byte[0];

            try
            {
                PacketDump.Create(data, -1);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
#if NETFRAMEWORK
                Assert.AreEqual(string.Format("Cannot be less than zero.{0}Parameter name: {1}", Environment.NewLine, ex.ParamName), ex.Message);
#else
                Assert.AreEqual(string.Format("Cannot be less than zero. (Parameter '{1}')", Environment.NewLine, ex.ParamName), ex.Message);
#endif
                Assert.AreEqual("indentLevel", ex.ParamName);
            }
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsEmpty()
        {
            var data = new byte[0];

            var actual = PacketDump.Create(data, 2);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsMultipleOfLineWidth_IndentLevelTwo()
        {
            var data = new byte[]
                {
                    0x07, 0x00, 0x1f, 0x65, 0x20, 0x62, 0x09, 0x44, 0x7f, 0x0d, 0x0a, 0x36, 0x80, 0x53, 0x53, 0x48,
                    0x2e, 0x4e, 0x45, 0x54, 0x20, 0x32, 0x30, 0x32, 0x30, 0x20, 0xf6, 0x7a, 0x32, 0x7f, 0x1f, 0x7e
                };
            var expected = "  00000000  07 00 1F 65 20 62 09 44 7F 0D 0A 36 80 53 53 48  ...e b.D...6.SSH" +
                           Environment.NewLine +
                           "  00000010  2E 4E 45 54 20 32 30 32 30 20 F6 7A 32 7F 1F 7E  .NET 2020 .z2..~";

            var actual = PacketDump.Create(data, 2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsMultipleOfLineWidth_IndentLevelZero()
        {
            var data = new byte[]
                {
                    0x07, 0x00, 0x1f, 0x65, 0x20, 0x62, 0x09, 0x44, 0x7f, 0x0d, 0x0a, 0x36, 0x80, 0x53, 0x53, 0x48,
                    0x2e, 0x4e, 0x45, 0x54, 0x20, 0x32, 0x30, 0x32, 0x30, 0x20, 0xf6, 0x7a, 0x32, 0x7f, 0x1f, 0x7e
                };
            var expected = "00000000  07 00 1F 65 20 62 09 44 7F 0D 0A 36 80 53 53 48  ...e b.D...6.SSH" +
                           Environment.NewLine +
                           "00000010  2E 4E 45 54 20 32 30 32 30 20 F6 7A 32 7F 1F 7E  .NET 2020 .z2..~";

            var actual = PacketDump.Create(data, 0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsLineWith()
        {
            var data = new byte[]
                {
                    0x07, 0x00, 0x1f, 0x65, 0x20, 0x62, 0x09, 0x44, 0x7f, 0x0d, 0x0a, 0x36, 0x80, 0x53, 0x53, 0x48
                };
            var expected = "  00000000  07 00 1F 65 20 62 09 44 7F 0D 0A 36 80 53 53 48  ...e b.D...6.SSH";

            var actual = PacketDump.Create(data, 2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsLessThanLineWith()
        {
            var data = new byte[]
                {
                    0x07, 0x00, 0x1f, 0x65, 0x20, 0x62, 0x09, 0x44, 0x7f, 0x0d, 0x0a, 0x36, 0x80, 0x53
                };
            var expected = "  00000000  07 00 1F 65 20 62 09 44 7F 0D 0A 36 80 53        ...e b.D...6.S";

            var actual = PacketDump.Create(data, 2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsGreaterThanLineWidthButLessThanMultipleOfLineWidth()
        {
            var data = new byte[]
                {
                    0x07, 0x00, 0x1f, 0x65, 0x20, 0x62, 0x09, 0x44, 0x7f, 0x0d, 0x0a, 0x36, 0x80, 0x53, 0x53, 0x48,
                    0x2e, 0x4e, 0x45, 0x54
                };
            var expected = "  00000000  07 00 1F 65 20 62 09 44 7F 0D 0A 36 80 53 53 48  ...e b.D...6.SSH" +
                           Environment.NewLine +
                           "  00000010  2E 4E 45 54                                      .NET";

            var actual = PacketDump.Create(data, 2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Create_ByteArrayAndIndentLevel_DataIsGreaterThanMultipleOfLineWidth()
        {
            var data = new byte[]
                {
                    0x07, 0x00, 0x1f, 0x65, 0x20, 0x62, 0x09, 0x44, 0x7f, 0x0d, 0x0a, 0x36, 0x80, 0x53, 0x53, 0x48,
                    0x2e, 0x4e, 0x45, 0x54, 0x20, 0x32, 0x30, 0x32, 0x30, 0x20, 0xf6, 0x7a, 0x32, 0x7f, 0x1f, 0x7e,
                    0x78, 0x54, 0x00, 0x52
                };
            var expected = "  00000000  07 00 1F 65 20 62 09 44 7F 0D 0A 36 80 53 53 48  ...e b.D...6.SSH" +
                           Environment.NewLine +
                           "  00000010  2E 4E 45 54 20 32 30 32 30 20 F6 7A 32 7F 1F 7E  .NET 2020 .z2..~" +
                           Environment.NewLine +
                           "  00000020  78 54 00 52                                      xT.R";

            var actual = PacketDump.Create(data, 2);

            Assert.AreEqual(expected, actual);

        }
    }
}
