namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Custom AES Cipher Mode, follows System.Security.Cryptography.CipherMode.
    /// </summary>
    public enum AesCipherMode
    {
        /// <summary>CBC Mode.</summary>
        CBC = 1,

        /// <summary>ECB Mode.</summary>
        ECB = 2,

        /// <summary>OFB Mode.</summary>
        OFB = 3,

        /// <summary>CFB Mode.</summary>
        CFB = 4,

        /// <summary>CTS Mode.</summary>
        CTS = 5,

        /// <summary>CTR Mode.</summary>
        CTR = 6
    }
}
