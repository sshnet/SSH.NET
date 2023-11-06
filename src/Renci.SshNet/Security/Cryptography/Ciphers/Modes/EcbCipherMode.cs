#pragma warning disable CA5358 // Review cipher mode usage with cryptography experts

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements EBC cipher mode.
    /// </summary>
    public class EcbCipherMode : CipherMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EcbCipherMode"/> class.
        /// </summary>
        public EcbCipherMode()
            : base(new byte[16], System.Security.Cryptography.CipherMode.ECB)
        {
        }
    }
}
