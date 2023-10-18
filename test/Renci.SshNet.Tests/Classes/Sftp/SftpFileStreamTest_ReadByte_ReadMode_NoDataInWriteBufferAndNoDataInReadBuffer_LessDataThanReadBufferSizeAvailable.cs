using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    /// Test for issue #173.
    /// </summary>
    [TestClass]
    public class SftpFileStreamTest_ReadByte_ReadMode_NoDataInWriteBufferAndNoDataInReadBuffer_LessDataThanReadBufferSizeAvailable : SftpFileStreamTestBase
    {
        private SftpFileStream _target;
        private string _path;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private int _actual;
        private byte[] _data;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(5, random);
            _bufferSize = (uint) random.Next(5, 1000);
            _readBufferSize = (uint) random.Next(10, 100);
            _writeBufferSize = (uint) random.Next(10, 100);
            _data = GenerateRandom((int) _readBufferSize - 2, random);
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
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize))
                           .Returns(_data);
        }

        [TestCleanup]
        public void TearDown()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestClose(_handle));
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object,
                                         _path,
                                         FileMode.Open,
                                         FileAccess.Read,
                                         (int)_bufferSize);
        }

        protected override void Act()
        {
            _actual = _target.ReadByte();
        }

        [TestMethod]
        public void ReadByteShouldReturnFirstByteThatWasReadFromServer()
        {
            Assert.AreEqual(_data[0], _actual);
        }

        [TestMethod]
        public void PositionShouldReturnOne()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(1L, _target.Position);
        }
    }
}
