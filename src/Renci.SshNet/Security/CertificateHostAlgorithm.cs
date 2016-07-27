using System;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Implements certificate support for host algorithm.
    /// </summary>
    public class CertificateHostAlgorithm : HostAlgorithm
    {
        /// <summary>
        /// Gets the host key data.
        /// </summary>
        public override byte[] Data
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The host key name.</param>
        public CertificateHostAlgorithm(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Signs the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Signed data.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override byte[] Sign(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><c>true</c> if signature was successfully verified; otherwise <c>false</c>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool VerifySignature(byte[] data, byte[] signature)
        {
            throw new NotImplementedException();
        }
    }
}
