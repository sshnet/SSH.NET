using System;
using System.IO;
using System.Linq;
using Renci.SshNet.Abstractions;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Renci.SshNet.Tests.Abstractions
{
    [TestClass]
    public class FileSystemAbstraction_EnumerateFiles
    {
        private string _temporaryDirectory;

        [TestInitialize]
        public void SetUp()
        {
            _temporaryDirectory = Path.GetTempFileName();
            File.Delete(_temporaryDirectory);
            Directory.CreateDirectory(_temporaryDirectory);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_temporaryDirectory != null && Directory.Exists(_temporaryDirectory))
            {
                Directory.Delete(_temporaryDirectory, true);
            }
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenDirectoryInfoIsNull()
        {
            const DirectoryInfo directoryInfo = null;
            const string searchPattern = "*.xml";

            try
            {
                FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("directoryInfo", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenSearchPatternIsNull()
        {
            var directoryInfo = new DirectoryInfo(_temporaryDirectory);
            const string searchPattern = null;

            try
            {
                FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("searchPattern", ex.ParamName);
            }
        }

        [TestMethod]
        public void ShouldThrowDirectoryNotFoundExceptionWhenDirectoryDoesNotExist()
        {
            var directoryInfo = new DirectoryInfo(_temporaryDirectory);
            const string searchPattern = "*.xml";

            Directory.Delete(_temporaryDirectory, true);

            try
            {
                FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern);
                Assert.Fail();
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        [TestMethod]
        public void ShouldReturnEmptyEnumerableWhenNoFilesExistInDirectory()
        {
            var directoryInfo = new DirectoryInfo(_temporaryDirectory);
            const string searchPattern = "*.xml";

            var actual = FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern);

            Assert.IsFalse(actual.GetEnumerator().MoveNext());
        }

        [TestMethod]
        public void ShouldReturnEmptyEnumerableWhenNoFilesMatchSearchPatternExistInDirectory()
        {
            CreateFile(Path.Combine(_temporaryDirectory, "test.txt"));

            var directoryInfo = new DirectoryInfo(_temporaryDirectory);
            const string searchPattern = "*.xml";

            var actual = FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern);

            Assert.IsFalse(actual.GetEnumerator().MoveNext());
        }

        [TestMethod]
        public void ShouldReturnEmptyEnumerableWhenSearchPatternIsEmpty()
        {
            CreateFile(Path.Combine(_temporaryDirectory, "test.txt"));

            var directoryInfo = new DirectoryInfo(_temporaryDirectory);
            const string searchPattern = "";

            var actual = FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern);

            Assert.IsFalse(actual.GetEnumerator().MoveNext());
        }

        [TestMethod]
        public void ShouldReturnAllFilesInDirectoryWhenSearchPatternIsAsterisk()
        {
            CreateFile(Path.Combine(_temporaryDirectory, "test.txt"));
            CreateFile(Path.Combine(_temporaryDirectory, "test.xml"));

            var directoryInfo = new DirectoryInfo(_temporaryDirectory);
            const string searchPattern = "*";

            var actual = FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern).ToList();

            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(actual.Exists(p => p.Name == "test.txt"));
            Assert.IsTrue(actual.Exists(p => p.Name == "test.xml"));
        }

        [TestMethod]
        public void ShouldReturnOnlyReturnFilesFromTopLevelDirectory()
        {
            CreateFile(Path.Combine(_temporaryDirectory, "test.txt"));
            CreateFile(Path.Combine(_temporaryDirectory, "test.xml"));
            Directory.CreateDirectory(Path.Combine(_temporaryDirectory, "sub"));
            CreateFile(Path.Combine(_temporaryDirectory, "sub", "test.csv"));

            var directoryInfo = new DirectoryInfo(_temporaryDirectory);
            const string searchPattern = "*";

            var actual = FileSystemAbstraction.EnumerateFiles(directoryInfo, searchPattern).ToList();

            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(actual.Exists(p => p.Name == "test.txt"));
            Assert.IsTrue(actual.Exists(p => p.Name == "test.xml"));
        }

        private static void CreateFile(string fileName)
        {
            var fs = File.Create(fileName);
            fs.Dispose();
        }
    }
}
