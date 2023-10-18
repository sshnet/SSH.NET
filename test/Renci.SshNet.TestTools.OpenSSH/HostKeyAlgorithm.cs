namespace Renci.SshNet.TestTools.OpenSSH
{
	public class HostKeyAlgorithm
    {
		public static readonly HostKeyAlgorithm EcdsaSha2Nistp256CertV01OpenSSH = new HostKeyAlgorithm("ecdsa-sha2-nistp256-cert-v01@openssh.com");
		public static readonly HostKeyAlgorithm EcdsaSha2Nistp384CertV01OpenSSH = new HostKeyAlgorithm("ecdsa-sha2-nistp384-cert-v01@openssh.com");
		public static readonly HostKeyAlgorithm EcdsaSha2Nistp521CertV01OpenSSH = new HostKeyAlgorithm("ecdsa-sha2-nistp521-cert-v01@openssh.com");
		public static readonly HostKeyAlgorithm SshEd25519CertV01OpenSSH = new HostKeyAlgorithm("ssh-ed25519-cert-v01@openssh.com");
		public static readonly HostKeyAlgorithm RsaSha2256CertV01OpenSSH = new HostKeyAlgorithm("rsa-sha2-256-cert-v01@openssh.com");
		public static readonly HostKeyAlgorithm RsaSha2512CertV01OpenSSH = new HostKeyAlgorithm("rsa-sha2-512-cert-v01@openssh.com");
		public static readonly HostKeyAlgorithm SshRsaCertV01OpenSSH = new HostKeyAlgorithm("ssh-rsa-cert-v01@openssh.com");
		public static readonly HostKeyAlgorithm EcdsaSha2Nistp256 = new HostKeyAlgorithm("ecdsa-sha2-nistp256");
		public static readonly HostKeyAlgorithm EcdsaSha2Nistp384 = new HostKeyAlgorithm("ecdsa-sha2-nistp384");
		public static readonly HostKeyAlgorithm EcdsaSha2Nistp521 = new HostKeyAlgorithm("ecdsa-sha2-nistp521");
		public static readonly HostKeyAlgorithm SshEd25519 = new HostKeyAlgorithm("ssh-ed25519");
		public static readonly HostKeyAlgorithm RsaSha2512 = new HostKeyAlgorithm("rsa-sha2-512");
		public static readonly HostKeyAlgorithm RsaSha2256 = new HostKeyAlgorithm("rsa-sha2-256");
		public static readonly HostKeyAlgorithm SshRsa = new HostKeyAlgorithm("ssh-rsa");
        public static readonly HostKeyAlgorithm SshDss = new HostKeyAlgorithm("ssh-dss");

        public HostKeyAlgorithm(string name)
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
