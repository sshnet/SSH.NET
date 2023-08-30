using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SFTP client integration tests
    /// </summary>
    [TestClass]
    public class SftpClientTests : IntegrationTestBase, IDisposable
    {
        private readonly SftpClient _sftpClient;

        public SftpClientTests()
        {
            _sftpClient = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            _sftpClient.Connect();
        }

        [TestMethod]
        public void Create_directory_with_contents_and_list_it()
        {
            var testDirectory = "/home/sshnet/sshnet-test";
            var testFileName =  "test-file.txt";
            var testFilePath = $"{testDirectory}/{testFileName}";
            var testContent = "file content";

            // Create new directory and check if it exists
            _sftpClient.CreateDirectory(testDirectory);
            Assert.IsTrue(_sftpClient.Exists(testDirectory));

            // Upload file and check if it exists
            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            _sftpClient.UploadFile(fileStream, testFilePath);
            Assert.IsTrue(_sftpClient.Exists(testFilePath));

            // Check if ListDirectory works
            var expectedFiles = new List<(string FullName, bool IsRegularFile, bool IsDirectory)>()
            {
                ("/home/sshnet/sshnet-test/.", IsRegularFile: false, IsDirectory: true),
                ("/home/sshnet/sshnet-test/..", IsRegularFile: false, IsDirectory: true),
                ("/home/sshnet/sshnet-test/test-file.txt", IsRegularFile: true, IsDirectory: false),
            };

            var actualFiles = _sftpClient.ListDirectory(testDirectory)
                .Select(f => (f.FullName, f.IsRegularFile, f.IsDirectory))
                .ToList();

            _sftpClient.DeleteFile(testFilePath);
            _sftpClient.DeleteDirectory(testDirectory);

            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        public async Task Create_directory_with_contents_and_list_it_async()
        {
            var testDirectory = "/home/sshnet/sshnet-test";
            var testFileName = "test-file.txt";
            var testFilePath = $"{testDirectory}/{testFileName}";
            var testContent = "file content";

            // Create new directory and check if it exists
            _sftpClient.CreateDirectory(testDirectory);
            Assert.IsTrue(_sftpClient.Exists(testDirectory));

            // Upload file and check if it exists
            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            _sftpClient.UploadFile(fileStream, testFilePath);
            Assert.IsTrue(_sftpClient.Exists(testFilePath));

            // Check if ListDirectory works
            var expectedFiles = new List<(string FullName, bool IsRegularFile, bool IsDirectory)>()
            {
                ("/home/sshnet/sshnet-test/.", IsRegularFile: false, IsDirectory: true),
                ("/home/sshnet/sshnet-test/..", IsRegularFile: false, IsDirectory: true),
                ("/home/sshnet/sshnet-test/test-file.txt", IsRegularFile: true, IsDirectory: false),
            };

            var actualFiles = new List<(string FullName, bool IsRegularFile, bool IsDirectory)>();

            await foreach (var file in _sftpClient.ListDirectoryAsync(testDirectory, CancellationToken.None))
            {
                actualFiles.Add((file.FullName, file.IsRegularFile, file.IsDirectory));
            }

            _sftpClient.DeleteFile(testFilePath);
            _sftpClient.DeleteDirectory(testDirectory);

            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
        }

        [TestMethod]
        [ExpectedException(typeof(SftpPermissionDeniedException), "Permission denied")]
        public void Test_Sftp_ListDirectory_Permission_Denied()
        {
            _sftpClient.ListDirectory("/root");
        }

        public void Dispose()
        {
            _sftpClient.Disconnect();
            _sftpClient.Dispose();
        }
    }
}
