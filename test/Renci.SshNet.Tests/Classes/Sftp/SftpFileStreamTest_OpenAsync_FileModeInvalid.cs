using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_OpenAsync_FileModeInvalid : SftpFileStreamAsyncTestBase
    {
        private Random _random;
        private string _path;
        private FileMode _fileMode;
        private FileAccess _fileAccess;
        private int _bufferSize;
        private ArgumentOutOfRangeException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString(CultureInfo.InvariantCulture);
            _fileMode = (FileMode) 666;
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
                await SftpFileStream.OpenAsync(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize, default)
                                    .ConfigureAwait(continueOnCapturedContext: false);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void CtorShouldHaveThrownArgumentException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("mode", _actualException.ParamName);
        }
    }
}
