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
        /// <exception cref="NotImplementedException">Always.</exception>
        public override byte[] Data
        {
            get
            {
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new NotImplementedException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
            }
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
        /// <exception cref="NotImplementedException">Always.</exception>
        public override byte[] Sign(byte[] data)
        {
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
            throw new NotImplementedException();
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns><see langword="true"/> if signature was successfully verified; otherwise <see langword="false"/>.</returns>
        /// <exception cref="NotImplementedException">Always.</exception>
        public override bool VerifySignature(byte[] data, byte[] signature)
        {
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
            throw new NotImplementedException();
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
        }
    }
}
