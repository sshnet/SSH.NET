using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Sftp.Responses;
using System.Threading;


namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    /// - In read mode
    /// - Bytes read from (read) buffer
    /// - New length greater than client position and greater than server position
    /// </summary>
    [TestClass]
    public class SftpFileStreamTest_SetLength_DataInReadBuffer_NewLengthGreatherThanPosition : SftpFileStreamTestBase
    {
        private string _path;
        private SftpFileStream _sftpFileStream;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private MockSequence _sequence;
        private long _length;

        private SftpFileAttributes _fileAttributes;
        private SftpFileAttributes _originalFileAttributes;
        private SftpFileAttributes _newFileAttributes;
        private byte[] _readBytes1;
        private byte[] _readBytes2;
        private byte[] _actualReadBytes;

        protected override void SetupData()
        {
            var random = new Random();

            _path = random.Next().ToString(CultureInfo.InvariantCulture);
            _handle = GenerateRandom(random.Next(2, 6), random);
            _bufferSize = (uint) random.Next(1, 1000);
            _readBytes1 = new byte[5];
            _readBytes2 = new byte[random.Next(1, 3)];
            _actualReadBytes = GenerateRandom(_readBytes1.Length + _readBytes2.Length + 2, random); // server returns more bytes than the caller requested
            _readBufferSize = (uint) random.Next(_actualReadBytes.Length, _actualReadBytes.Length * 2);
            _writeBufferSize = (uint) random.Next(100, 1000);
            _length = _readBytes1.Length + _readBytes2.Length + 5;

            _fileAttributes = new SftpFileAttributesBuilder().WithExtension("X", "ABC")
                                                             .WithExtension("V", "VValue")
                                                             .WithGroupId(random.Next())
                                                             .WithLastAccessTime(DateTime.UtcNow.AddSeconds(random.Next()))
                                                             .WithLastWriteTime(DateTime.UtcNow.AddSeconds(random.Next()))
                                                             .WithPermissions((uint) random.Next())
                                                             .WithSize(_length + 100)
                                                             .WithUserId(random.Next())
                                                             .Build();
            _originalFileAttributes = _fileAttributes.Clone();
            _newFileAttributes = null;
        }

        protected override void SetupMocks()
        {
            _sequence = new MockSequence();
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write, false))
                           .Returns(_handle);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                           .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.RequestRead(_handle, 0, _readBufferSize))
                           .Returns(_actualReadBytes);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.RequestFStat(_handle, false))
                           .Returns(_fileAttributes);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.RequestFSetStat(_handle, _fileAttributes))
                           .Callback<byte[], SftpFileAttributes>((bytes, attributes) => _newFileAttributes = attributes.Clone());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _sftpFileStream = new SftpFileStream(SftpSessionMock.Object,
                                                 _path,
                                                 FileMode.Open,
                                                 FileAccess.ReadWrite,
                                                 (int) _bufferSize);
            _sftpFileStream.Read(_readBytes1, 0, _readBytes1.Length);
            _sftpFileStream.Read(_readBytes2, 0, _readBytes2.Length); // this will return bytes from the buffer
        }

        protected override void Act()
        {
            _sftpFileStream.SetLength(_length);
        }

        [TestMethod]
        public void PositionShouldReturnSamePositionAsBeforeSetLength()
        {
            SftpSessionMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(_readBytes1.Length + _readBytes2.Length, _sftpFileStream.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(4));
        }

        [TestMethod]
        public void RequestFSetStatOnSftpSessionShouldBeInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.RequestFSetStat(_handle, _fileAttributes), Times.Once);
        }

        [TestMethod]
        public void SizeOfSftpFileAttributesShouldBeModifiedToNewLengthBeforePassedToRequestFSetStat()
        {
            DictionaryAssert.AreEqual(_originalFileAttributes.Extensions, _newFileAttributes.Extensions);
            Assert.AreEqual(_originalFileAttributes.GroupId, _newFileAttributes.GroupId);
            Assert.AreEqual(_originalFileAttributes.LastAccessTime, _newFileAttributes.LastAccessTime);
            Assert.AreEqual(_originalFileAttributes.LastWriteTime, _newFileAttributes.LastWriteTime);
            Assert.AreEqual(_originalFileAttributes.Permissions, _newFileAttributes.Permissions);
            Assert.AreEqual(_originalFileAttributes.UserId, _newFileAttributes.UserId);

            Assert.AreEqual(_length, _newFileAttributes.Size);
        }

        [TestMethod]
        public void ReadShouldReadStartFromSamePositionAsBeforeSetLength()
        {
            SftpSessionMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.RequestRead(_handle,
                                                     (uint) (_readBytes1.Length + _readBytes2.Length),
                                                     _readBufferSize))
                           .Returns(new byte[] {0x0f});

            var byteRead = _sftpFileStream.ReadByte();

            Assert.AreEqual(0x0f, byteRead);

            SftpSessionMock.Verify(p => p.RequestRead(_handle,
                                                      (uint) (_readBytes1.Length + _readBytes2.Length),
                                                      _readBufferSize),
                                   Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(4));
        }

        [TestMethod]
        public void WriteShouldStartFromSamePositionAsBeforeSetLength()
        {
            var bytesToWrite = GenerateRandom(_writeBufferSize);
            byte[] bytesWritten = null;

            SftpSessionMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(_sequence)
                           .Setup(p => p.RequestWrite(_handle,
                                                      (uint) (_readBytes1.Length + _readBytes2.Length),
                                                      It.IsAny<byte[]>(),
                                                      0,
                                                      bytesToWrite.Length,
                                                      It.IsAny<AutoResetEvent>(),
                                                      null))
                           .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>(
                               (handle, serverOffset, data, offset, length, wait, writeCompleted) =>
                               {
                                   bytesWritten = data.Take(offset, length);
                                   wait.Set();
                               });

            _sftpFileStream.Write(bytesToWrite, 0, bytesToWrite.Length);

            Assert.IsNotNull(bytesWritten);
            CollectionAssert.AreEqual(bytesToWrite, bytesWritten);

            SftpSessionMock.Verify(p => p.RequestWrite(_handle,
                                                       (uint) (_readBytes1.Length + _readBytes2.Length),
                                                       It.IsAny<byte[]>(),
                                                       0,
                                                       bytesToWrite.Length,
                                                       It.IsAny<AutoResetEvent>(),
                                                       null),
                                   Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(4));
        }
    }
}
