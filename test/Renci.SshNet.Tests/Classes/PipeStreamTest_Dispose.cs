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
            Assert.IsFalse(_pipeStream.CanRead);
        }

        [TestMethod]
        public void Flush_ShouldThrowObjectDisposedException()
        {
            try
            {
                _pipeStream.Flush();
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void MaxBufferLength_Getter_ShouldReturnTwoHundredMegabyte()
        {
            Assert.AreEqual(200 * 1024 * 1024, _pipeStream.MaxBufferLength);
        }

        [TestMethod]
        public void MaxBufferLength_Setter_ShouldModifyMaxBufferLength()
        {
            var newValue = new Random().Next(1, int.MaxValue);
            _pipeStream.MaxBufferLength = newValue;
            Assert.AreEqual(newValue, _pipeStream.MaxBufferLength);
        }

        [TestMethod]
        public void Length_ShouldThrowObjectDisposedException()
        {
            try
            {
                var value = _pipeStream.Length;
                Assert.Fail("" + value);
            }
            catch (ObjectDisposedException)
            {
            }
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
        public void Read_ByteArrayAndOffsetAndCount_ShouldThrowObjectDisposedException()
        {
            var buffer = new byte[0];
            const int offset = 0;
            const int count = 0;

            try
            {
                _pipeStream.Read(buffer, offset, count);
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void ReadByte_ShouldThrowObjectDisposedException()
        {
            try
            {
                _pipeStream.ReadByte();
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
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
