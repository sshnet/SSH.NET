using System;
using System.Globalization;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Ctor_FileModeCreateNew_FileAccessRead : SftpFileStreamTestBase
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
            _path = _random.Next().ToString(CultureInfo.InvariantCulture);
            _fileMode = FileMode.CreateNew;
            _fileAccess = FileAccess.Read;
            _bufferSize = _random.Next(5, 1000);
        }

        protected override void SetupMocks()
        {
        }

        protected override void Act()
        {
            SftpFileStream fs = null;

            try
            {
                fs = new SftpFileStream(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                _actualException = ex;
            }
            finally
            {
                fs?.Dispose();
            }
        }

        [TestMethod]
        public void CtorShouldHaveThrownArgumentException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual(string.Format("Combining {0}: {1} with {2}: {3} is invalid.", nameof(FileMode), _fileMode, nameof(FileAccess), _fileAccess), _actualException.Message);
            Assert.IsNull(_actualException.ParamName);
        }
    }
}
