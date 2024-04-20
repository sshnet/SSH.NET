namespace Renci.SshNet.IntegrationTests
{
    public sealed class HostKeyFile
    {
        public static readonly HostKeyFile Rsa = new HostKeyFile("ssh-rsa", "/etc/ssh/ssh_host_rsa_key", new byte[] { 0x3d, 0x90, 0xd8, 0x0d, 0xd5, 0xe0, 0xb6, 0x13, 0x42, 0x7c, 0x78, 0x1e, 0x19, 0xa3, 0x99, 0x2b });
        public static readonly HostKeyFile Dsa = new HostKeyFile("ssh-dsa", "/etc/ssh/ssh_host_dsa_key", new byte[] { 0x50, 0xe0, 0xd5, 0x11, 0xf7, 0xed, 0x54, 0x75, 0x0d, 0x03, 0xc6, 0x52, 0x9b, 0x3b, 0x3c, 0x9f });
        public static readonly HostKeyFile Ed25519 = new HostKeyFile("ssh-ed25519", "/etc/ssh/ssh_host_ed25519_key", new byte[] { 0xb3, 0xb9, 0xd0, 0x1b, 0x73, 0xc4, 0x60, 0xb4, 0xce, 0xed, 0x06, 0xf8, 0x58, 0x49, 0xa3, 0xda });
        public const string Ecdsa = "/etc/ssh/ssh_host_ecdsa_key";

        private HostKeyFile(string keyName, string filePath, byte[] fingerPrint)
        {
            KeyName = keyName;
            FilePath = filePath;
            FingerPrint = fingerPrint;
        }

        public string KeyName { get; }
        public string FilePath { get; }
        public byte[] FingerPrint { get; }
    }


}
