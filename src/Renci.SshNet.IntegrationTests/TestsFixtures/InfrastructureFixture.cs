using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Renci.SshNet.IntegrationTests.TestsFixtures
{
    public sealed class InfrastructureFixture : IAsyncLifetime, IDisposable
    {
        private IContainer? _sshServer;

        private IFutureDockerImage? _sshServerImage;

        public string? SshServerHostName { get; set; }

        public ushort SshServerPort { get; set; }

        // TODO the user name and password can be injected to dockerfile via arguments
        public SshUser AdminUser = new SshUser("sshnetadm", "ssh4ever");

        // TODO the user name and password can be injected to dockerfile via arguments
        public SshUser User = new SshUser("sshnet", "ssh4ever");

        public async Task InitializeAsync()
        {
            //TestcontainersSettings.Logger = new TestLogger();

            _sshServerImage = new ImageFromDockerfileBuilder()
                .WithName("renci-ssh-tests-server-image")
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "Renci.SshNet.IntegrationTests")
                .WithDockerfile("Dockerfile")
                .WithDeleteIfExists(true)
                
                .Build();

            await _sshServerImage.CreateAsync();

            _sshServer = new ContainerBuilder()
                .WithHostname("renci-ssh-tests-server")
                .WithImage(_sshServerImage)
                //.WithPrivileged(true)
                .WithPortBinding(22, true)
                .Build();

            await _sshServer.StartAsync();

            SshServerPort = _sshServer.GetMappedPublicPort(22);
            SshServerHostName = _sshServer.Hostname;
        }

        public async Task DisposeAsync()
        {
            if (_sshServer != null)
            {
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
