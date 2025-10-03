using System.Runtime.InteropServices;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

using Microsoft.Extensions.Logging;

namespace Renci.SshNet.IntegrationTests.TestsFixtures
{
    public sealed class InfrastructureFixture : IDisposable
    {
        private InfrastructureFixture()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddFilter("testcontainers", LogLevel.Information);
                builder.AddConsole();
            });

            SshNetLoggingConfiguration.InitializeLogging(_loggerFactory);
        }

        public static InfrastructureFixture Instance { get; } = new InfrastructureFixture();

        private readonly ILoggerFactory _loggerFactory;

        private IContainer _sshServer;

        private IFutureDockerImage _sshServerImage;

        public string SshServerHostName { get; private set; }

        public ushort SshServerPort { get; private set; }

        public SshUser AdminUser { get; } = new SshUser("sshnetadm", "ssh4ever");

        public SshUser User { get; } = new SshUser("sshnet", "ssh4ever");

        public async Task InitializeAsync()
        {
#pragma warning disable MA0144 // use System.OperatingSystem to check the current OS
            // for the Windows Tests in CI, the Container is set up in WSL2 with Podman
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                Environment.GetEnvironmentVariable("CI") == "true")
#pragma warning restore MA0144 // use System.OperatingSystem to check the current OS
            {
                SshServerPort = 2222;
                SshServerHostName = "127.0.0.1";
                await Task.Delay(1_000);
                return;
            }

            var containerLogger = _loggerFactory.CreateLogger("testcontainers");

            _sshServerImage = new ImageFromDockerfileBuilder()
                .WithName("renci-ssh-tests-server-image")
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), Path.Combine("test", "Renci.SshNet.IntegrationTests"))
                .WithDockerfile("Dockerfile")
                .WithDeleteIfExists(true)
                .WithLogger(containerLogger)
                .Build();

            await _sshServerImage.CreateAsync();

            _sshServer = new ContainerBuilder()
                .WithHostname("renci-ssh-tests-server")
                .WithImage(_sshServerImage)
                .WithPortBinding(22, true)
                .WithLogger(containerLogger)
                .Build();

            await _sshServer.StartAsync();

            SshServerPort = _sshServer.GetMappedPublicPort(22);
            SshServerHostName = _sshServer.Hostname;

            // Socket fails on Linux, reporting inability early. This is the Linux behavior by design.
            // https://github.com/dotnet/runtime/issues/47484#issuecomment-769239699
            // At this point we have to wait until the ssh server in the container is available
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                await Task.Delay(300);
            }
        }

        public async Task DisposeAsync()
        {
            if (_sshServer != null)
            {
#pragma warning disable S6966 // Awaitable method should be used
                //try
                //{
                //    File.WriteAllBytes(@"C:\tmp\auth.log", await _sshServer.ReadFileAsync("/var/log/auth.log").ConfigureAwait(false));
                //}
                //catch (Exception ex)
                //{
                //    Console.Error.WriteLine(ex.ToString());
                //}
#pragma warning restore S6966 // Awaitable method should be used

                await _sshServer.DisposeAsync();
            }

            if (_sshServerImage != null)
            {
                await _sshServerImage.DisposeAsync();
            }
        }

        public void Dispose()
        {
        }
    }
}
