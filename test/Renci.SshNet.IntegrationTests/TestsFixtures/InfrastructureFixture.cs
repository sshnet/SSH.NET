using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace Renci.SshNet.IntegrationTests.TestsFixtures
{
    public sealed class InfrastructureFixture : IAsyncDisposable
    {
        private InfrastructureFixture()
        {
        }

        private static readonly Lazy<InfrastructureFixture> InstanceLazy = new Lazy<InfrastructureFixture>(() => new InfrastructureFixture());

        public static InfrastructureFixture Instance
        {
            get
            {
                return InstanceLazy.Value;
            }
        }

        private IContainer _sshServer;

        private IFutureDockerImage _sshServerImage;

        public string SshServerHostName { get; set; }

        public ushort SshServerPort { get; set; }

        public SshUser AdminUser { get; } = new SshUser("sshnetadm", "ssh4ever");

        public SshUser User { get; } = new SshUser("sshnet", "ssh4ever");

        public async Task InitializeAsync()
        {
            _sshServerImage = new ImageFromDockerfileBuilder()
                .WithName("renci-ssh-tests-server-image")
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), Path.Combine("test", "Renci.SshNet.IntegrationTests"))
                .WithDockerfile("Dockerfile")
                .WithDeleteIfExists(deleteIfExists: true)
                .Build();

            await _sshServerImage.CreateAsync().ConfigureAwait(continueOnCapturedContext: false);

            _sshServer = new ContainerBuilder()
                .WithHostname("renci-ssh-tests-server")
                .WithImage(_sshServerImage)
                .WithPortBinding(22, assignRandomHostPort: true)
                .Build();

            await _sshServer.StartAsync().ConfigureAwait(continueOnCapturedContext: false);

            SshServerPort = _sshServer.GetMappedPublicPort(22);
            SshServerHostName = _sshServer.Hostname;
        }

        public async ValueTask DisposeAsync()
        {
            if (_sshServer != null)
            {
                await _sshServer.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
            }

            if (_sshServerImage != null)
            {
                await _sshServerImage.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }
}
