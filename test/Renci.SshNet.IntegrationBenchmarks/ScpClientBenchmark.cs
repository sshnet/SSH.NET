using System.Text;
using BenchmarkDotNet.Attributes;

using Renci.SshNet.IntegrationTests.TestsFixtures;

namespace Renci.SshNet.IntegrationBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class ScpClientBenchmark : IntegrationBenchmarkBase
    {
        private readonly InfrastructureFixture _infrastructureFixture;

        private readonly string _file = $"/tmp/{Guid.NewGuid()}.txt";
        private ScpClient? _scpClient;
        private MemoryStream? _uploadStream;

        public ScpClientBenchmark()
        {
            _infrastructureFixture = InfrastructureFixture.Instance;
        }

        [GlobalSetup]
        public async Task Setup()
        {
            await GlobalSetup().ConfigureAwait(false);
            _scpClient = new ScpClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await _scpClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

            var fileContent = "File content !@#$%^&*()_+{}:,./<>[];'\\|";
            _uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            await GlobalCleanup().ConfigureAwait(false);
            await _uploadStream!.DisposeAsync().ConfigureAwait(false);
        }

        [Benchmark]
        public void Connect()
        {
            using var scpClient = new ScpClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            scpClient.Connect();
        }

        [Benchmark]
        public async Task ConnectAsync()
        {
            using var scpClient = new ScpClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await scpClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public string ConnectUploadAndDownload()
        {
            using var scpClient = new ScpClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            scpClient.Connect();
            _uploadStream!.Position = 0;
            scpClient.Upload(_uploadStream, _file);
            using var downloadStream = new MemoryStream();
            scpClient.Download(_file, downloadStream);

            return Encoding.UTF8.GetString(downloadStream.ToArray());
        }

        [Benchmark]
        public string UploadAndDownload()
        {
            _uploadStream!.Position = 0;
            _scpClient!.Upload(_uploadStream, _file);
            using var downloadStream = new MemoryStream();
            _scpClient.Download(_file, downloadStream);

            return Encoding.UTF8.GetString(downloadStream.ToArray());
        }
    }
}
