using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class PipeStreamTest_Dispose : TestBase
    {
        private PipeStream _pipeStream;

        protected override void OnInit()
        {
            base.OnInit();

            Arrange();
            Act();
        }

        private void Arrange()
        {
            _pipeStream = new PipeStream();
        }

        private void Act()
        {
            _pipeStream.Dispose();
        }

        [TestMethod]
        public void CanRead_ShouldReturnTrue()
        {
            Assert.IsTrue(_pipeStream.CanRead);
        }

        [TestMethod]
        public void Flush_ShouldNotThrow()
        {
            _pipeStream.Flush();
        }

        [TestMethod]
        public void Length_ShouldNotThrow()
        {
            _ = _pipeStream.Length;
        }

        [TestMethod]
        public void Position_Getter_ShouldReturnZero()
        {
            Assert.AreEqual(0, _pipeStream.Position);
        }

        [TestMethod]
        public void Position_Setter_ShouldThrowNotSupportedException()
        {
            try
            {
                _pipeStream.Position = 0;
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        public void Read_ByteArrayAndOffsetAndCount_ShouldNotThrow()
        {
            Assert.AreEqual(0, _pipeStream.Read(new byte[1], 0, 1));
        }

        [TestMethod]
        public void ReadByte_ShouldNotThrow()
        {
            Assert.AreEqual(-1, _pipeStream.ReadByte());
        }

        [TestMethod]
        public void Seek_ShouldThrowNotSupportedException()
        {
            try
            {
                _pipeStream.Seek(0, SeekOrigin.Begin);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        public void SetLength_ShouldThrowNotSupportedException()
        {
            try
            {
                _pipeStream.SetLength(0);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        public void Write_ByteArrayAndOffsetAndCount_ShouldThrowObjectDisposedException()
        {
            var buffer = new byte[0];
            const int offset = 0;
            const int count = 0;

            try
            {
                _pipeStream.Write(buffer, offset, count);
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void WriteByte_ShouldThrowObjectDisposedException()
        {
            const byte b = 0x0a;

            try
            {
                _pipeStream.WriteByte(b);
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}
