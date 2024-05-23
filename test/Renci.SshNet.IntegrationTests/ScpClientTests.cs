namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SCP client integration tests
    /// </summary>
    [TestClass]
    public class ScpClientTests : IntegrationTestBase, IDisposable
    {
        private readonly ScpClient _scpClient;

        public ScpClientTests()
        {
            _scpClient = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            _scpClient.Connect();
        }

        [TestMethod]

        public void Upload_And_Download_FileStream()
        {
            var file = $"/tmp/{Guid.NewGuid()}.txt";
            var fileContent = "File content !@#$%^&*()_+{}:,./<>[];'\\|";

            using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            _scpClient.Upload(uploadStream, file);

            using var downloadStream = new MemoryStream();
            _scpClient.Download(file, downloadStream);

            var result = Encoding.UTF8.GetString(downloadStream.ToArray());

            Assert.AreEqual(fileContent, result);
        }

        public void Dispose()
        {
            _scpClient.Disconnect();
            _scpClient.Dispose();
        }
    }
}
