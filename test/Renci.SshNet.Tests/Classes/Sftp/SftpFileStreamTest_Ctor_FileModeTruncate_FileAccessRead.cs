using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    internal class SftpFileStreamTest_Ctor_FileModeTruncate_FileAccessRead : SftpFileStreamTestBase
    {
        private Random _random;
        private string _path;
        private FileMode _fileMode;
        private FileAccess _fileAccess;
        private int _bufferSize;
        private ArgumentException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString();
            _fileMode = FileMode.Truncate;
            _fileAccess = FileAccess.Read;
            _bufferSize = _random.Next(5, 1000);
        }

        protected override void SetupMocks()
        {
        }

        protected override void Act()
        {
            try
            {
                _ = new SftpFileStream(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void CtorShouldHaveThrownArgumentException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            ArgumentExceptionAssert.MessageEquals(string.Format("Combining {0}: {1} with {2}: {3} is invalid.", nameof(FileMode), _fileMode, nameof(FileAccess), _fileAccess), _actualException);
            Assert.AreEqual("mode", _actualException.ParamName);
        }
    }
}
