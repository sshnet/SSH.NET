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
    public class SftpFileStreamTest_Flush_ReadMode_DataInBuffer_ReadFromBuffer : SftpFileStreamTestBase
    {
        private SftpFileStream _target;
        private string _path;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private byte[] _readBytes1;
        private byte[] _readBytes2;
        private byte[] _serverBytes;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(5, random);
            _bufferSize = (uint)random.Next(1, 1000);
            _readBufferSize = 100;
            _writeBufferSize = 500;
            _readBytes1 = new byte[random.Next(1, (int) _readBufferSize - 10)];
            _readBytes2 = new byte[random.Next(1, 3)];
            _serverBytes = GenerateRandom(_readBytes1.Length + 10); // store 5 bytes in read buffer
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
            _target.Read(_readBytes1, 0, _readBytes1.Length);
            _target.Read(_readBytes2, 0, _readBytes2.Length);
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

            Assert.AreEqual(_readBytes1.Length + _readBytes2.Length, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(4));
        }

        [TestMethod]
        public void ReadShouldReadFromServer()
        {
            var serverBytes3 = GenerateRandom(5);
            var readBytes3 = new byte[3];
            var expectedReadBytes = new ArrayBuilder<byte>().Add(new byte[1])
                                                            .Add(serverBytes3.Take(0, 2))
                                                            .Build();

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestRead(_handle, (ulong) (_readBytes1.Length + _readBytes2.Length), _readBufferSize))
                           .Returns(serverBytes3);

            var bytesRead = _target.Read(readBytes3, 1, 2);

            Assert.AreEqual(2, bytesRead);
            CollectionAssert.AreEqual(expectedReadBytes, readBytes3);

            SftpSessionMock.Verify(p => p.RequestRead(_handle, (ulong)(_readBytes1.Length + _readBytes2.Length), _readBufferSize), Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(4));
        }
    }
}
