namespace Renci.SshNet.Security
{
    /// <summary>
    /// Base class for SSH host algorithms.
    /// </summary>
    public abstract class HostAlgorithm
    {
        /// <summary>
        /// Gets the host key name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the host key data.
        /// </summary>
        public abstract byte[] Data { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The host key name.</param>
        protected HostAlgorithm(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Signs the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Signed data.</returns>
        public abstract byte[] Sign(byte[] data);

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><c>True</c> is signature was successfully verifies; otherwise <c>false</c>.</returns>
        public abstract bool VerifySignature(byte[] data, byte[] signature);
    }
}
