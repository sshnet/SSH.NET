using BenchmarkDotNet.Attributes;

using Renci.SshNet.IntegrationTests.TestsFixtures;

namespace Renci.SshNet.IntegrationBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class SshClientBenchmark : IntegrationBenchmarkBase
    {
        private readonly InfrastructureFixture _infrastructureFixture;
        private SshClient? _sshClient;

        public SshClientBenchmark()
        {
            _infrastructureFixture = InfrastructureFixture.Instance;
        }

        [GlobalSetup]
        public async Task Setup()
        {
            await GlobalSetup().ConfigureAwait(false);
            _sshClient = new SshClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await _sshClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            await GlobalCleanup().ConfigureAwait(false);
        }

        [Benchmark]
        public void Connect()
        {
            using var sshClient = new SshClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            sshClient.Connect();
        }

        [Benchmark]
        public async Task ConnectAsync()
        {
            using var sshClient = new SshClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await sshClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
        }

        [Benchmark]
        public string ConnectAndRunCommand()
        {
            using var sshClient = new SshClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            sshClient.Connect();
            return sshClient.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'").Result;
        }

        [Benchmark]
        public async Task<string> ConnectAsyncAndRunCommand()
        {
            using var sshClient = new SshClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await sshClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
            return sshClient.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'").Result;
        }

        [Benchmark]
        public string RunCommand()
        {
            return _sshClient!.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'").Result;
        }
    }
}
