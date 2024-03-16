using System.Text;

using BenchmarkDotNet.Attributes;

using Renci.SshNet.IntegrationTests.TestsFixtures;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.IntegrationBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class SftpClientBenchmark : IntegrationBenchmarkBase
    {
        private readonly InfrastructureFixture _infrastructureFixture;
        private readonly string _file = $"/tmp/{Guid.NewGuid()}.txt";

        private SftpClient? _sftpClient;
        private MemoryStream? _uploadStream;

        public SftpClientBenchmark()
        {
            _infrastructureFixture = InfrastructureFixture.Instance;
        }

        [GlobalSetup]
        public async Task Setup()
        {
            await GlobalSetup().ConfigureAwait(false);
            _sftpClient = new SftpClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await _sftpClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

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
            using var sftpClient = new SftpClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            sftpClient.Connect();
        }

        [Benchmark]
        public async Task ConnectAsync()
        {
            using var sftpClient = new SftpClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await sftpClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
        }

        public IEnumerable<ISftpFile> ListDirectory()
        {
            return _sftpClient!.ListDirectory("/root");
        }

        public IAsyncEnumerable<ISftpFile> ListDirectoryAsync()
        {
            return _sftpClient!.ListDirectoryAsync("/root", CancellationToken.None);
        }

        [Benchmark]
        public string UploadAndDownload()
        {
            _uploadStream!.Position = 0;
            _sftpClient!.UploadFile(_uploadStream, _file);
            using var downloadStream = new MemoryStream();
            _sftpClient.DownloadFile(_file, downloadStream);

            return Encoding.UTF8.GetString(downloadStream.ToArray());
        }
    }
}
