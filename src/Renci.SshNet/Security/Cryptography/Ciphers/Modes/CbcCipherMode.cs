namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements CBC cipher mode.
    /// </summary>
    public class CbcCipherMode : CipherMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CbcCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public CbcCipherMode(byte[] iv)
            : base(iv, System.Security.Cryptography.CipherMode.CBC)
        {
        }
    }
}
