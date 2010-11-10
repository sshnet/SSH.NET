
namespace Renci.SshClient.Tests
{
    public static class ConnectionData
    {
        public static string Host { get { return "server"; } }

        public static int Port { get { return 22; } }

        public static string Username { get { return "username"; } }

        public static string Password { get { return "password"; } }

        public static string RsaKeyFilePath { get { return @"RSA key path"; } }

        public static string DssKeyFilePath { get { return @"DSS key path"; } }
    }
}
