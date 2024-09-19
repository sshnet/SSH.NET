namespace Renci.SshNet.IntegrationTests
{
    public sealed class HostCertificateFile
    {
        public static readonly HostCertificateFile RsaCertRsa = new HostCertificateFile("ssh-rsa-cert-v01@openssh.com", "/etc/ssh/ssh_host_rsa_key-cert_rsa.pub", HostKeyFile.Rsa, "x0vVk+h7SGE7bNN0wAA2vsA9Mg9qLOZPqhq2Dj/rqfM");
        public static readonly HostCertificateFile Ed25519CertEcdsa = new HostCertificateFile("ssh-ed25519-cert-v01@openssh.com", "/etc/ssh/ssh_host_ed25519_key-cert_ecdsa", HostKeyFile.Ed25519, "Z2diHpknyvJpetRw47iIjqt9OUzm6cAVOe4FM5FbDQw");
        public static readonly HostCertificateFile Ecdsa256CertRsa = new HostCertificateFile("ecdsa-sha2-nistp256-cert-v01@openssh.com", "/etc/ssh/ssh_host_ecdsa256_key-cert_rsa", HostKeyFile.Ecdsa256, "x0vVk+h7SGE7bNN0wAA2vsA9Mg9qLOZPqhq2Dj/rqfM");
        public static readonly HostCertificateFile Ecdsa384CertEcdsa = new HostCertificateFile("ecdsa-sha2-nistp384-cert-v01@openssh.com", "/etc/ssh/ssh_host_ecdsa384_key-cert_ecdsa", HostKeyFile.Ecdsa384, "Z2diHpknyvJpetRw47iIjqt9OUzm6cAVOe4FM5FbDQw");
        public static readonly HostCertificateFile Ecdsa521CertEd25519 = new HostCertificateFile("ecdsa-sha2-nistp521-cert-v01@openssh.com", "/etc/ssh/ssh_host_ecdsa521_key-cert_ed25519", HostKeyFile.Ecdsa521, "tF3DRTUXtYFZ5Yz0SBOrEbixHaCifHmNVK6FtptXZVM");

        private HostCertificateFile(string certificateName, string filePath, HostKeyFile hostKeyFile, string caFingerPrint)
        {
            CertificateName = certificateName;
            FilePath = filePath;
            HostKeyFile = hostKeyFile;
            CAFingerPrint = caFingerPrint;
        }

        public string CertificateName { get; }
        public string FilePath { get; }
        public HostKeyFile HostKeyFile { get; }
        public string CAFingerPrint { get; }
    }
}
