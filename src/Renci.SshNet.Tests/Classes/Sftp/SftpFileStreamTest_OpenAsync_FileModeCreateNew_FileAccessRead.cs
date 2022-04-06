#if FEATURE_TAP
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_OpenAsync_FileModeCreateNew_FileAccessRead : SftpFileStreamAsyncTestBase
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
            _fileMode = FileMode.CreateNew;
            _fileAccess = FileAccess.Read;
            _bufferSize = _random.Next(5, 1000);
        }

        protected override void SetupMocks()
        {
        }

        protected override async Task ActAsync()
        {
            try
            {
                await SftpFileStream.OpenAsync(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize, default);
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
            Assert.AreEqual(string.Format("Combining {0}: {1} with {2}: {3} is invalid.", typeof(FileMode).Name, _fileMode, typeof(FileAccess).Name, _fileAccess), _actualException.Message);
            Assert.IsNull(_actualException.ParamName);
        }
    }
}
#endif