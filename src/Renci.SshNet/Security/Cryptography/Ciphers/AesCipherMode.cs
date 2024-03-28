namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Custom AES Cipher Mode, follows System.Security.Cryptography.CipherMode.
    /// </summary>
    public enum AesCipherMode
    {
        /// <summary>Cipher Block Chain Mode.</summary>
        CBC = 1,

        /// <summary>Electronic Codebook Mode.</summary>
        ECB = 2,

        /// <summary>Output Feedback Mode.</summary>
        OFB = 3,

        /// <summary>Cipher Feedback Mode.</summary>
        CFB = 4,

        /// <summary>Cipher Text Stealing Mode.</summary>
        CTS = 5,

        /// <summary>Counter Mode.</summary>
        CTR = 6
    }
}
