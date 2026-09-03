#if NET // The test uses Parallel.ForEachAsync, which is not available on .NET Framework.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

using Microsoft.Extensions.Logging;

using Renci.SshNet.IntegrationTests.Logging;

namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// Reproduces https://github.com/sshnet/SSH.NET/issues/1764: connection failures during
    /// SFTP transfers when the server initiates a key re-exchange.
    /// <para>
    /// Unlike OpenSSH, which queues non key exchange output while a re-exchange is in progress,
    /// ProFTPD mod_sftp keeps sending channel messages (SSH_MSG_CHANNEL_WINDOW_ADJUST,
    /// SSH_MSG_CHANNEL_DATA) after it has sent its SSH_MSG_KEXINIT. The same load which passes
    /// against the OpenSSH test server therefore fails against ProFTPD with either
    /// "Message type 93 is not valid in the current context." or a connection drop
    /// ("Key exchange failed"). These tests run against a ProFTPD container configured to
    /// re-key every 1 MB (see proftpd/proftpd.conf) to give the race many trials per upload.
    /// </para>
    /// <para>
    /// Trace-level logging to a file is enabled for the duration of these tests: the small
    /// per-message overhead on the message listener thread widens the window between the
    /// arrival of the server's SSH_MSG_KEXINIT and its processing, during which concurrent
    /// uploaders keep sending data. This mirrors real-world conditions (applications with
    /// trace logging enabled, or slower links) and makes the race fail reliably on loopback.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class ProFtpdRekeyTests : TestBase
    {
        private static IFutureDockerImage _proFtpdImage;
        private static IContainer _proFtpdServer;
        private static string _proFtpdHostName;
        private static ushort _proFtpdPort;
        private static StreamWriter _traceLogWriter;
        private static ILoggerFactory _traceLoggerFactory;

        [ClassInitialize]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "MSTests requires context parameter")]
        public static async Task ClassInitialize(TestContext context)
        {
            // The Windows Tests in CI cannot run the ProFTPD container: Docker on the Windows
            // runners is in Windows containers mode ("no matching manifest for windows/amd64"),
            // which is why the OpenSSH server for the other integration tests is set up in
            // WSL2 with Podman instead (see InfrastructureFixture).
            if (OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("CI") == "true")
            {
                Assert.Inconclusive("Requires a container runtime able to run Linux containers.");
            }

            _traceLogWriter = new StreamWriter(Path.GetTempFileName()) { AutoFlush = true };
            _traceLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddProvider(new TextWriterLoggerProvider(_traceLogWriter));
            });

            SshNetLoggingConfiguration.InitializeLogging(_traceLoggerFactory);

            _proFtpdImage = new ImageFromDockerfileBuilder()
                .WithName("renci-ssh-tests-proftpd-image")
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), Path.Combine("test", "Renci.SshNet.IntegrationTests"))
                .WithDockerfile("proftpd/Dockerfile")
                .WithDeleteIfExists(true)
                .Build();

            await _proFtpdImage.CreateAsync(context.CancellationToken);

            _proFtpdServer = new ContainerBuilder(_proFtpdImage)
                .WithHostname("renci-ssh-tests-proftpd")
                .WithPortBinding(22, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(22))
                .Build();

            await _proFtpdServer.StartAsync(context.CancellationToken);

            _proFtpdPort = _proFtpdServer.GetMappedPublicPort(22);
            _proFtpdHostName = _proFtpdServer.Hostname;
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (_proFtpdServer != null)
            {
                await _proFtpdServer.DisposeAsync();
            }

            if (_proFtpdImage != null)
            {
                await _proFtpdImage.DisposeAsync();
            }

            // Restore the assembly-wide logging configuration set up by InfrastructureFixture.
            var defaultLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddTestConsoleLogger();
            });

            SshNetLoggingConfiguration.InitializeLogging(defaultLoggerFactory);

            _traceLoggerFactory?.Dispose();
            _traceLogWriter?.Dispose();
        }

        [TestMethod]
        public async Task Sftp_ConcurrentUploads_WithServerRekey()
        {
            const int fileSize = 128 * 1024 * 1024;
            const int concurrentUploads = 4;
            const int attempts = 3;

            using (var sftp = new SftpClient(_proFtpdHostName, _proFtpdPort, "sshnet", "ssh4ever"))
            {
                await sftp.ConnectAsync(CancellationToken.None);

                for (var attempt = 0; attempt < attempts; attempt++)
                {
                    await Parallel.ForEachAsync(Enumerable.Range(0, concurrentUploads), async (i, ct) =>
                    {
                        var localFile = CreateZeroFilledTempFile(fileSize);

                        try
                        {
                            var remoteFile = $"rekey-test-{i}";

                            using (var fileStream = File.OpenRead(localFile))
                            {
                                await sftp.UploadFileAsync(fileStream, remoteFile, ct);
                            }

                            var remoteLength = (await sftp.GetAsync(remoteFile, ct)).Attributes.Size;

                            Assert.AreEqual(fileSize, remoteLength);
                        }
                        finally
                        {
                            File.Delete(localFile);
                        }
                    });
                }
            }
        }

        private static string CreateZeroFilledTempFile(long size)
        {
            var file = Path.GetTempFileName();

            using (var fs = File.OpenWrite(file))
            {
                fs.SetLength(size);
            }

            return file;
        }
    }
}
#endif
