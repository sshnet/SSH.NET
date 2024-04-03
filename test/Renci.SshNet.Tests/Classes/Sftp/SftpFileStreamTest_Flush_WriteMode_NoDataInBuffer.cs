using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;
using Renci.SshNet.Tests.Common;
using System;
using System.IO;
using System.Threading;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Flush_WriteMode_NoDataInBuffer : SftpFileStreamTestBase
    {
        private SftpFileStream _target;
        private string _path;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private byte[] _writeBytes;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(5, random);
            _bufferSize = (uint)random.Next(1, 1000);
            _readBufferSize = 100;
            _writeBufferSize = 500;
            _writeBytes = GenerateRandom(_writeBufferSize);
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write, false))
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
                           .Setup(p => p.RequestWrite(_handle, 0UL, It.IsAny<byte[]>(), 0, _writeBytes.Length, It.IsAny<AutoResetEvent>(), null))
                           .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverOffset, data, offset, length, wait, writeCompleted)
                               =>
                           {
                               wait.Set();
                           });
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
                                         FileAccess.ReadWrite,
                                         (int) _bufferSize);
            _target.Write(_writeBytes, 0, _writeBytes.Length);
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

            Assert.AreEqual(_writeBytes.Length, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
        }

        [TestMethod]
        public void ReadShouldReadFromServer()
        {
            var serverBytes = GenerateRandom(5);
            var readBytes = new byte[5];
            var expectedReadBytes = new ArrayBuilder<byte>().Add(new byte[2])
                                                            .Add(serverBytes.Take(0, 3))
                                                            .Build();

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestRead(_handle, (ulong) _writeBytes.Length, _readBufferSize))
                           .Returns(serverBytes);

            var bytesRead = _target.Read(readBytes, 2, 3);

            Assert.AreEqual(3, bytesRead);
            CollectionAssert.AreEqual(expectedReadBytes, readBytes);

            SftpSessionMock.Verify(p => p.RequestRead(_handle, (ulong) _writeBytes.Length, _readBufferSize), Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
        }
    }
}
