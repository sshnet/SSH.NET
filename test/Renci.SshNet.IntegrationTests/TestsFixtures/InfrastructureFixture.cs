using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace Renci.SshNet.IntegrationTests.TestsFixtures
{
    public sealed class InfrastructureFixture : IDisposable
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

        public SshUser AdminUser = new SshUser("sshnetadm", "ssh4ever");

        public SshUser User = new SshUser("sshnet", "ssh4ever");

        // To get the sshd logs (also uncomment WithOutputConsumer below)
        private readonly Stream _fsOut = Stream.Null; // File.Create("fsout.txt");
        private readonly Stream _fsErr = Stream.Null; // File.Create("fserr.txt");

        public async Task InitializeAsync()
        {
            _sshServerImage = new ImageFromDockerfileBuilder()
                .WithName("renci-ssh-tests-server-image")
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), Path.Combine("test", "Renci.SshNet.IntegrationTests"))
                .WithDockerfile("Dockerfile.TestServer")
                .WithDeleteIfExists(true)
                .Build();

            await _sshServerImage.CreateAsync();

            _sshServer = new ContainerBuilder()
                .WithHostname("renci-ssh-tests-server")
                .WithImage(_sshServerImage)
                .WithPortBinding(22, true)
                //.WithOutputConsumer(Consume.RedirectStdoutAndStderrToStream(_fsOut, _fsErr))
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
                await _sshServer.DisposeAsync();
            }

            if (_sshServerImage != null)
            {
                await _sshServerImage.DisposeAsync();
            }

            _fsOut.Dispose();
            _fsErr.Dispose();
        }

        public void Dispose()
        {
        }
    }
}
