namespace Renci.SshNet.TestTools.OpenSSH
{
    public class MessageAuthenticationCodeAlgorithm
    {
        public static readonly MessageAuthenticationCodeAlgorithm HmacMd5 = new MessageAuthenticationCodeAlgorithm("hmac-md5");
        public static readonly MessageAuthenticationCodeAlgorithm HmacMd5_96 = new MessageAuthenticationCodeAlgorithm("hmac-md5-96");
        public static readonly MessageAuthenticationCodeAlgorithm HmacRipemd160 = new MessageAuthenticationCodeAlgorithm("hmac-ripemd160");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha1 = new MessageAuthenticationCodeAlgorithm("hmac-sha1");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha1_96 = new MessageAuthenticationCodeAlgorithm("hmac-sha1-96");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha2_256 = new MessageAuthenticationCodeAlgorithm("hmac-sha2-256");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha2_512 = new MessageAuthenticationCodeAlgorithm("hmac-sha2-512");
        public static readonly MessageAuthenticationCodeAlgorithm Umac64 = new MessageAuthenticationCodeAlgorithm("umac-64@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm Umac128 = new MessageAuthenticationCodeAlgorithm("umac-128@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm HmacMd5Etm = new MessageAuthenticationCodeAlgorithm("hmac-md5-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm HmacMd5_96_Etm = new MessageAuthenticationCodeAlgorithm("hmac-md5-96-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm HmacRipemd160Etm = new MessageAuthenticationCodeAlgorithm("hmac-ripemd160-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha1Etm = new MessageAuthenticationCodeAlgorithm("hmac-sha1-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha1_96_Etm = new MessageAuthenticationCodeAlgorithm("hmac-sha1-96-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha2_256_Etm = new MessageAuthenticationCodeAlgorithm("hmac-sha2-256-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm HmacSha2_512_Etm = new MessageAuthenticationCodeAlgorithm("hmac-sha2-512-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm Umac64_Etm = new MessageAuthenticationCodeAlgorithm("umac-64-etm@openssh.com");
        public static readonly MessageAuthenticationCodeAlgorithm Umac128_Etm = new MessageAuthenticationCodeAlgorithm("umac-128-etm@openssh.com");

        public MessageAuthenticationCodeAlgorithm(string name)
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

            if (obj is MessageAuthenticationCodeAlgorithm otherMac)
            {
                return otherMac.Name == Name;
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
