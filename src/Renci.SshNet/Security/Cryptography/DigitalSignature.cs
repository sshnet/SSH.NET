namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for signature implementations.
    /// </summary>
#pragma warning disable S1694 // An abstract class should have both abstract and concrete methods
    public abstract class DigitalSignature
#pragma warning restore S1694 // An abstract class should have both abstract and concrete methods
    {
        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        /// <see langword="true"/> if signature was successfully verified; otherwise <see langword="false"/>.
        /// </returns>
        public abstract bool Verify(byte[] input, byte[] signature);

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Signed input data.</returns>
        public abstract byte[] Sign(byte[] input);
    }
}
