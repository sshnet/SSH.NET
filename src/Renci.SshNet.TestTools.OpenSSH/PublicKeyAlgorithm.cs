namespace Renci.SshNet.TestTools.OpenSSH
{
    public class PublicKeyAlgorithm
    {
        public static readonly PublicKeyAlgorithm SshEd25519 = new PublicKeyAlgorithm("ssh-ed25519");
        public static readonly PublicKeyAlgorithm SshEd25519CertV01OpenSSH = new PublicKeyAlgorithm("ssh-ed25519-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm SkSshEd25519OpenSSH = new PublicKeyAlgorithm("sk-ssh-ed25519@openssh.com");
        public static readonly PublicKeyAlgorithm SkSshEd25519CertV01OpenSSH = new PublicKeyAlgorithm("sk-ssh-ed25519-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm SshRsa = new PublicKeyAlgorithm("ssh-rsa");
        public static readonly PublicKeyAlgorithm RsaSha2256 = new PublicKeyAlgorithm("rsa-sha2-256");
        public static readonly PublicKeyAlgorithm RsaSha2512 = new PublicKeyAlgorithm("rsa-sha2-512");
        public static readonly PublicKeyAlgorithm SshDss = new PublicKeyAlgorithm("ssh-dss");
        public static readonly PublicKeyAlgorithm EcdsaSha2Nistp256 = new PublicKeyAlgorithm("ecdsa-sha2-nistp256");
        public static readonly PublicKeyAlgorithm EcdsaSha2Nistp384 = new PublicKeyAlgorithm("ecdsa-sha2-nistp384");
        public static readonly PublicKeyAlgorithm EcdsaSha2Nistp521 = new PublicKeyAlgorithm("ecdsa-sha2-nistp521");
        public static readonly PublicKeyAlgorithm SkEcdsaSha2Nistp256OpenSSH = new PublicKeyAlgorithm("sk-ecdsa-sha2-nistp256@openssh.com");
        public static readonly PublicKeyAlgorithm WebAuthnSkEcdsaSha2Nistp256OpenSSH = new PublicKeyAlgorithm("webauthn-sk-ecdsa-sha2-nistp256@openssh.com");
        public static readonly PublicKeyAlgorithm SshRsaCertV01OpenSSH = new PublicKeyAlgorithm("ssh-rsa-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm RsaSha2256CertV01OpenSSH = new PublicKeyAlgorithm("rsa-sha2-256-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm RsaSha2512CertV01OpenSSH = new PublicKeyAlgorithm("rsa-sha2-512-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm SshDssCertV01OpenSSH = new PublicKeyAlgorithm("ssh-dss-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm EcdsaSha2Nistp256CertV01OpenSSH = new PublicKeyAlgorithm("ecdsa-sha2-nistp256-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm EcdsaSha2Nistp384CertV01OpenSSH = new PublicKeyAlgorithm("ecdsa-sha2-nistp384-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm EcdsaSha2Nistp521CertV01OpenSSH = new PublicKeyAlgorithm("ecdsa-sha2-nistp521-cert-v01@openssh.com");
        public static readonly PublicKeyAlgorithm SkEcdsaSha2Nistp256CertV01OpenSSH = new PublicKeyAlgorithm("sk-ecdsa-sha2-nistp256-cert-v01@openssh.com");

        public PublicKeyAlgorithm(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is HostKeyAlgorithm otherHka)
            {
                return otherHka.Name == Name;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
