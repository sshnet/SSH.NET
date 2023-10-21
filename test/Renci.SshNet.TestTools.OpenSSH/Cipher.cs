namespace Renci.SshNet.TestTools.OpenSSH
{
    public sealed class Cipher
    {
        public static readonly Cipher TripledesCbc = new Cipher("3des-cbc");
        public static readonly Cipher Aes128Cbc = new Cipher("aes128-cbc");
        public static readonly Cipher Aes192Cbc = new Cipher("aes192-cbc");
        public static readonly Cipher Aes256Cbc = new Cipher("aes256-cbc");
        public static readonly Cipher RijndaelCbc = new Cipher("rijndael-cbc@lysator.liu.se");
        public static readonly Cipher Aes128Ctr = new Cipher("aes128-ctr");
        public static readonly Cipher Aes192Ctr = new Cipher("aes192-ctr");
        public static readonly Cipher Aes256Ctr = new Cipher("aes256-ctr");
        public static readonly Cipher Aes128Gcm = new Cipher("aes128-gcm@openssh.com");
        public static readonly Cipher Aes256Gcm = new Cipher("aes256-gcm@openssh.com");
        public static readonly Cipher Arcfour = new Cipher("arcfour");
        public static readonly Cipher Arcfour128 = new Cipher("arcfour128");
        public static readonly Cipher Arcfour256 = new Cipher("arcfour256");
        public static readonly Cipher BlowfishCbc = new Cipher("blowfish-cbc");
        public static readonly Cipher Cast128Cbc = new Cipher("cast128-cbc");
        public static readonly Cipher Chacha20Poly1305 = new Cipher("chacha20-poly1305@openssh.com");

        public Cipher(string name)
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

            if (obj is Cipher otherCipher)
            {
                return otherCipher.Name == Name;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
