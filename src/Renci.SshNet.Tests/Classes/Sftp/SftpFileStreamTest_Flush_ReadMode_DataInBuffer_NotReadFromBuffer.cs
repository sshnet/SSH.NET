using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;
using System;
using System.IO;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Flush_ReadMode_DataInBuffer_NotReadFromBuffer : SftpFileStreamTestBase
    {
        private SftpFileStream _target;
        private string _path;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private byte[] _readBytes;
        private byte[] _serverBytes;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(5, random);
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = 100;
            _writeBufferSize = 500;
            _readBytes = new byte[random.Next(1, (int) _readBufferSize - 10)];
            _serverBytes = GenerateRandom(_readBytes.Length + 5); // store 5 bytes in read buffer
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpen(_path, Flags.Read, false))
                           .Returns(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                           .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize))
                           .Returns(_serverBytes);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object,
                                         _path,
                                         FileMode.Open,
                                         FileAccess.Read,
                                         (int)_bufferSize);
            _target.Read(_readBytes, 0, _readBytes.Length);
        }

        protected override void Act()
        {
            _target.Flush();
        }

        [TestMethod]
        public void PositionShouldReturnSameValueAsBeforeFlush()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);

            Assert.AreEqual(_readBytes.Length, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
        }

        [TestMethod]
        public void ReadShouldReadFromServer()
        {
            var serverBytes2 = GenerateRandom(5);
            var readBytes2 = new byte[5];
            var expectedReadBytes = new ArrayBuilder<byte>().Add(new byte[2])
                                                            .Add(serverBytes2.Take(0, 3))
                                                            .Build();

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestRead(_handle, (ulong)_readBytes.Length, _readBufferSize))
                           .Returns(serverBytes2);

            var bytesRead = _target.Read(readBytes2, 2, 3);

            Assert.AreEqual(3, bytesRead);
            CollectionAssert.AreEqual(expectedReadBytes, readBytes2);

            SftpSessionMock.Verify(p => p.RequestRead(_handle, (ulong)_readBytes.Length, _readBufferSize), Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
        }
    }
}
