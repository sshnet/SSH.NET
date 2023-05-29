using FluentAssertions;
using System.Text;
using Xunit.Abstractions;

namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SCP client integration tests
    /// </summary>
    [Collection("Infrastructure collection")]
    public class ScpClientTests : IntegrationTestBase, IDisposable
    {
        private readonly ScpClient _scpClient;

        public ScpClientTests(ITestOutputHelper testOutputHelper, InfrastructureFixture infrastructureFixture)
            : base(testOutputHelper, infrastructureFixture)
        {
            _scpClient = new ScpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            _scpClient.Connect();
        }

        [Fact]

        public void Scp_Upload_And_Download_FileStream()
        {
            var file = $"/tmp/{Guid.NewGuid()}.txt";
            var fileContent = "File content !@#$%^&*()_+{}:,./<>[];'\\|";
        
            using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            _scpClient.Upload(uploadStream, file);

            using var downloadStream = new MemoryStream();
            _scpClient.Download(file, downloadStream);

            var result = Encoding.UTF8.GetString(downloadStream.ToArray());

            result.Should().Be(fileContent);
        }

        public void Dispose()
        {
            _scpClient.Disconnect();
            _scpClient.Dispose();
        }
    }
}
