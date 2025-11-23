using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests.Common
{
    internal static class RemoteSshdConfigExtensions
    {
        private const string DefaultAuthenticationMethods = "password publickey";

        public static void Reset(this RemoteSshdConfig remoteSshdConfig)
        {
            remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, DefaultAuthenticationMethods)
                            .WithChallengeResponseAuthentication(false)
                            .WithKeyboardInteractiveAuthentication(false)
                            .PrintMotd()
                            .WithLogLevel(LogLevel.Debug3)
                            .ClearHostKeyFiles()
                            .AddHostKeyFile(HostKeyFile.Rsa.FilePath)
                            .WithHostKeyCertificate(null)
                            .ClearSubsystems()
                            .AddSubsystem(new Subsystem("sftp", "/usr/lib/ssh/sftp-server"))
                            .ClearCiphers()
                            .ClearKeyExchangeAlgorithms()
                            .ClearHostKeyAlgorithms()
                            .ClearPublicKeyAcceptedAlgorithms()
                            .ClearMessageAuthenticationCodeAlgorithms()
                            .PermitTTY(true)
                            .WithUsePAM(true)
                            .Update()
                            .Restart();
        }
    }
}
