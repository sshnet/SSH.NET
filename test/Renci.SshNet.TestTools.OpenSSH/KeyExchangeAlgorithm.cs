namespace Renci.SshNet.TestTools.OpenSSH
{
    public class KeyExchangeAlgorithm
    {
        public static readonly KeyExchangeAlgorithm DiffieHellmanGroup1Sha1 = new KeyExchangeAlgorithm("diffie-hellman-group1-sha1");
        public static readonly KeyExchangeAlgorithm DiffieHellmanGroup14Sha1 = new KeyExchangeAlgorithm("diffie-hellman-group14-sha1");
        public static readonly KeyExchangeAlgorithm DiffieHellmanGroup14Sha256 = new KeyExchangeAlgorithm("diffie-hellman-group14-sha256");
        public static readonly KeyExchangeAlgorithm DiffieHellmanGroup16Sha512 = new KeyExchangeAlgorithm("diffie-hellman-group16-sha512");
        public static readonly KeyExchangeAlgorithm DiffieHellmanGroup18Sha512 = new KeyExchangeAlgorithm("diffie-hellman-group18-sha512");
        public static readonly KeyExchangeAlgorithm DiffieHellmanGroupExchangeSha1 = new KeyExchangeAlgorithm("diffie-hellman-group-exchange-sha1");
        public static readonly KeyExchangeAlgorithm DiffieHellmanGroupExchangeSha256 = new KeyExchangeAlgorithm("diffie-hellman-group-exchange-sha256");
        public static readonly KeyExchangeAlgorithm EcdhSha2Nistp256 = new KeyExchangeAlgorithm("ecdh-sha2-nistp256");
        public static readonly KeyExchangeAlgorithm EcdhSha2Nistp384 = new KeyExchangeAlgorithm("ecdh-sha2-nistp384");
        public static readonly KeyExchangeAlgorithm EcdhSha2Nistp521 = new KeyExchangeAlgorithm("ecdh-sha2-nistp521");
        public static readonly KeyExchangeAlgorithm Curve25519Sha256 = new KeyExchangeAlgorithm("curve25519-sha256");
        public static readonly KeyExchangeAlgorithm Curve25519Sha256Libssh = new KeyExchangeAlgorithm("curve25519-sha256@libssh.org");
        public static readonly KeyExchangeAlgorithm Sntrup4591761x25519Sha512 = new KeyExchangeAlgorithm("sntrup4591761x25519-sha512@tinyssh.org");


        public KeyExchangeAlgorithm(string name)
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

            if (obj is KeyExchangeAlgorithm otherKex)
            {
                return otherKex.Name == Name;
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
