namespace Renci.SshNet.IntegrationTests
{
    public sealed class HostKeyFile
    {
        public static readonly HostKeyFile Rsa = new HostKeyFile("ssh-rsa", "/etc/ssh/ssh_host_rsa_key", 3072, new byte[] { 0x3d, 0x90, 0xd8, 0x0d, 0xd5, 0xe0, 0xb6, 0x13, 0x42, 0x7c, 0x78, 0x1e, 0x19, 0xa3, 0x99, 0x2b });
        public static readonly HostKeyFile Dsa = new HostKeyFile("ssh-dsa", "/etc/ssh/ssh_host_dsa_key", 1024, new byte[] { 0xcc, 0xb4, 0x4c, 0xe1, 0xba, 0x6d, 0x15, 0x79, 0xec, 0xe1, 0x31, 0x9f, 0xc0, 0x4a, 0x07, 0x9d });
        public static readonly HostKeyFile Ed25519 = new HostKeyFile("ssh-ed25519", "/etc/ssh/ssh_host_ed25519_key", 256, new byte[] { 0xb3, 0xb9, 0xd0, 0x1b, 0x73, 0xc4, 0x60, 0xb4, 0xce, 0xed, 0x06, 0xf8, 0x58, 0x49, 0xa3, 0xda });
        public static readonly HostKeyFile Ecdsa256 = new HostKeyFile("ecdsa-sha2-nistp256", "/etc/ssh/ssh_host_ecdsa256_key", 256, new byte[] { 0xbe, 0x98, 0xa1, 0x54, 0x91, 0x2c, 0x96, 0xc3, 0x77, 0x39, 0x6e, 0x37, 0x8e, 0x64, 0x26, 0x72 });
        public static readonly HostKeyFile Ecdsa384 = new HostKeyFile("ecdsa-sha2-nistp384", "/etc/ssh/ssh_host_ecdsa384_key", 384, new byte[] { 0xab, 0xbb, 0x20, 0x07, 0x3c, 0xb2, 0x89, 0x9e, 0x40, 0xfe, 0x32, 0x56, 0xfe, 0xd9, 0x95, 0x0b });
        public static readonly HostKeyFile Ecdsa521 = new HostKeyFile("ecdsa-sha2-nistp521", "/etc/ssh/ssh_host_ecdsa521_key", 521, new byte[] { 0x31, 0xed, 0x9c, 0x89, 0x6f, 0xa3, 0xe4, 0x0d, 0x68, 0x6a, 0xe6, 0xde, 0x89, 0x39, 0x08, 0x7d });

        private HostKeyFile(string keyName, string filePath, int keyLength, byte[] fingerPrint)
        {
            KeyName = keyName;
            FilePath = filePath;
            KeyLength = keyLength;
            FingerPrint = fingerPrint;
        }

        public string KeyName { get; }
        public string FilePath { get; }
        public int KeyLength { get; }
        public byte[] FingerPrint { get; }
    }


}
