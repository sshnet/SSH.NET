using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PipeStreamTest : TestBase
    {
        [TestMethod]
        [TestCategory("PipeStream")]
        public void Test_PipeStream_Write_Read_Buffer()
        {
            var testBuffer = new byte[1024];
            new Random().NextBytes(testBuffer);

            var outputBuffer = new byte[1024];

            using (var stream = new PipeStream())
            {
                stream.Write(testBuffer, 0, testBuffer.Length);

                Assert.AreEqual(stream.Length, testBuffer.Length);

                stream.Read(outputBuffer, 0, outputBuffer.Length);

                Assert.AreEqual(stream.Length, 0);

                Assert.IsTrue(testBuffer.IsEqualTo(outputBuffer));
            }
        }

        [TestMethod]
        [TestCategory("PipeStream")]
        public void Test_PipeStream_Write_Read_Byte()
        {
            var testBuffer = new byte[1024];
            new Random().NextBytes(testBuffer);

            var outputBuffer = new byte[1024];

            using (var stream = new PipeStream())
            {
                stream.Write(testBuffer, 0, testBuffer.Length);

                Assert.AreEqual(stream.Length, testBuffer.Length);

                stream.ReadByte();

                Assert.AreEqual(stream.Length, testBuffer.Length - 1);

                stream.ReadByte();

                Assert.AreEqual(stream.Length, testBuffer.Length - 2);
            }
        }
    }
}