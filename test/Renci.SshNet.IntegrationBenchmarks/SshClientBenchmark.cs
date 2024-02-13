using BenchmarkDotNet.Attributes;

using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.TestsFixtures;

namespace Renci.SshNet.IntegrationBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class SshClientBenchmark : IntegrationBenchmarkBase
    {
        private static readonly Dictionary<TerminalModes, uint> ShellStreamTerminalModes = new Dictionary<TerminalModes, uint>
        {
            { TerminalModes.ECHO, 0 }
        };

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
        public async Task<string> ConnectAndRunCommandAsync()
        {
            using var sshClient = new SshClient(_infrastructureFixture.SshServerHostName, _infrastructureFixture.SshServerPort, _infrastructureFixture.User.UserName, _infrastructureFixture.User.Password);
            await sshClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
            var result = await sshClient.RunCommandAsync("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'", CancellationToken.None).ConfigureAwait(false);
            return result.Result;
        }

        [Benchmark]
        public string RunCommand()
        {
            return _sshClient!.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'").Result;
        }

        [Benchmark]
        public async Task<string> RunCommandAsync()
        {
            var result = await _sshClient!.RunCommandAsync("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'", CancellationToken.None).ConfigureAwait(false);
            return result.Result;
        }

        [Benchmark]
        public string ShellStreamReadLine()
        {
            using (var shellStream = _sshClient!.CreateShellStream("xterm", 80, 24, 800, 600, 1024, ShellStreamTerminalModes))
            {
                shellStream.WriteLine("for i in $(seq 500); do echo \"Within cells. Interlinked. $i\"; sleep 0.001; done; echo \"Username:\";");

                while (true)
                {
                    var line = shellStream.ReadLine();

                    if (line.EndsWith("500", StringComparison.Ordinal))
                    {
                        return line;
                    }
                }
            }
        }

        [Benchmark]
        public string ShellStreamExpect()
        {
            using (var shellStream = _sshClient!.CreateShellStream("xterm", 80, 24, 800, 600, 1024, ShellStreamTerminalModes))
            {
                shellStream.WriteLine("for i in $(seq 500); do echo \"Within cells. Interlinked. $i\"; sleep 0.001; done; echo \"Username:\";");
                return shellStream.Expect("Username:");
            }
        }
    }
}
