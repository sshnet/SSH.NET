
namespace Renci.SshClient.Tests
{
    public static class ConnectionData
    {
        public static string Host { get { return "oleg-centos.edc.renci.org"; } }

        public static int Port { get { return 22; } }

        public static string Username { get { return "tester"; } }

        public static string Password { get { return "tester"; } }

        public static string RsaKeyFilePath { get { return @"RSA key path"; } }

        public static string DssKeyFilePath { get { return @"DSS key path"; } }
    }
}
