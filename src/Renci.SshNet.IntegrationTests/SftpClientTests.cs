namespace IntegrationTests
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
            var files = _sftpClient.ListDirectory(testDirectory);

            _sftpClient.DeleteFile(testFilePath);
            _sftpClient.DeleteDirectory(testDirectory);

            var builder = new StringBuilder();
            foreach (var file in files)
            {
                builder.AppendLine($"{file.FullName} {file.IsRegularFile} {file.IsDirectory}");
            }

            Assert.AreEqual(@"/home/sshnet/sshnet-test/. False True
/home/sshnet/sshnet-test/.. False True
/home/sshnet/sshnet-test/test-file.txt True False
", builder.ToString());
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
            var files = await _sftpClient.ListDirectoryAsync(testDirectory, CancellationToken.None);

            _sftpClient.DeleteFile(testFilePath);
            _sftpClient.DeleteDirectory(testDirectory);

            var builder = new StringBuilder();
            foreach (var file in files)
            {
                builder.AppendLine($"{file.FullName} {file.IsRegularFile} {file.IsDirectory}");
            }

            Assert.AreEqual(@"/home/sshnet/sshnet-test/. False True
/home/sshnet/sshnet-test/.. False True
/home/sshnet/sshnet-test/test-file.txt True False
", builder.ToString());
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
