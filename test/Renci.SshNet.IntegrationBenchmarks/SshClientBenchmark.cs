using BenchmarkDotNet.Attributes;

using Renci.SshNet.IntegrationTests.TestsFixtures;

namespace Renci.SshNet.IntegrationBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class SshClientBenchmark : IntegrationBenchmarkBase
    {
        private readonly InfrastructureFixture _infrastructureFixture;

        public SshClientBenchmark()
        {
            _infrastructureFixture = InfrastructureFixture.Instance;
        }

        [GlobalSetup]
        public async Task Setup()
        {
            await GlobalSetup().ConfigureAwait(false);
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            await GlobalCleanup().ConfigureAwait(false);
        }

        [Benchmark]
        public string Connect()
        {
            using var sshClient = new SshClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            sshClient.Connect();
            return sshClient.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'").Result;
        }
    }
}
