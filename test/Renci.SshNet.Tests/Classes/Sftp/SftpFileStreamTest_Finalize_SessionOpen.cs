using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Finalize_SessionOpen : SftpFileStreamTestBase
    {
        private WeakReference<SftpFileStream> _target;
        private string _path;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString(CultureInfo.InvariantCulture);
            _handle = GenerateRandom(7, random);
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = (uint) random.Next(1, 1000);
            _writeBufferSize = (uint) random.Next(1, 1000);
        }

        protected override void SetupMocks()
        {
            _ = SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.CreateNewOrOpen, false))
                               .Returns(_handle);
            _ = SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                               .Returns(_readBufferSize);
            _ = SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                               .Returns(_writeBufferSize);
            _ = SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.IsOpen)
                               .Returns(true);
            _ = SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.RequestClose(_handle));
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = CreateWeakSftpFileStream();
        }

        protected override void Act()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void SftpFileStreamShouldHaveBeenFinalized()
        {
            Assert.IsFalse(_target.TryGetTarget(out _));
        }

        [TestMethod]
        public void IsOpenOnSftpSessionShouldNeverBeInvoked()
        {
            SftpSessionMock.Verify(p => p.IsOpen, Times.Never);
        }

        [TestMethod]
        public void RequestCloseOnSftpSessionShouldNeverBeInvoked()
        {
            SftpSessionMock.Verify(p => p.RequestClose(_handle), Times.Never);
        }

        private WeakReference<SftpFileStream> CreateWeakSftpFileStream()
        {
            var sftpFileStream = new SftpFileStream(SftpSessionMock.Object,
                                                    _path,
                                                    FileMode.OpenOrCreate,
                                                    FileAccess.ReadWrite,
                                                    (int) _bufferSize);
            return new WeakReference<SftpFileStream>(sftpFileStream);
        }
    }
}
