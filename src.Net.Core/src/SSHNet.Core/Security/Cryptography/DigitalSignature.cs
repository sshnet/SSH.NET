namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for signature implementations
    /// </summary>
    public abstract class DigitalSignature
    {
        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><c>True</c> if signature was successfully verified; otherwise <c>false</c>.</returns>
        public abstract bool Verify(byte[] input, byte[] signature);

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Signed input data.</returns>
        public abstract byte[] Sign(byte[] input);
    }
}
