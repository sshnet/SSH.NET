using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests.Common
{
    internal static class RemoteSshdConfigExtensions
    {
        private const string DefaultAuthenticationMethods = "password publickey";

        public static void Reset(this RemoteSshdConfig remoteSshdConfig)
        {
            remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, DefaultAuthenticationMethods)
                            .WithChallengeResponseAuthentication(value: false)
                            .WithKeyboardInteractiveAuthentication(value: false)
                            .PrintMotd()
                            .WithLogLevel(LogLevel.Debug3)
                            .ClearHostKeyFiles()
                            .AddHostKeyFile(HostKeyFile.Rsa.FilePath)
                            .ClearSubsystems()
                            .AddSubsystem(new Subsystem("sftp", "/usr/lib/ssh/sftp-server"))
                            .ClearCiphers()
                            .ClearKeyExchangeAlgorithms()
                            .ClearHostKeyAlgorithms()
                            .ClearPublicKeyAcceptedAlgorithms()
                            .ClearMessageAuthenticationCodeAlgorithms()
                            .WithUsePAM(usePAM: true)
                            .Update()
                            .Restart();
        }
    }
}
