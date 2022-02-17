using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_CanRead_Closed_FileAccessRead : SftpFileStreamTestBase
    {
        private SftpFileStream _target;
        private string _path;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private bool _actual;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(3, random);
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = (uint) random.Next(0, 1000);
            _writeBufferSize = (uint) random.Next(0, 1000);
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
                           .Setup(p => p.RequestClose(_handle));
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object,
                                         _path,
                                         FileMode.Open,
                                         FileAccess.Read,
                                         (int) _bufferSize);
            _target.Close();
        }

        protected override void Act()
        {
            _actual = _target.CanRead;
        }

        [TestMethod]
        public void CanReadShouldReturnFalse()
        {
            Assert.IsFalse(_actual);
        }
    }
}
