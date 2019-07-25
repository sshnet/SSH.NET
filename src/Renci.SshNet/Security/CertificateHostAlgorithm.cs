using System;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Implements certificate support for host algorithm.
    /// </summary>
    public class CertificateHostAlgorithm : KeyHostAlgorithm
    {
        private readonly byte[] _data;

        /// <summary>
        /// Gets the host key data.
        /// </summary>
        public override byte[] Data
        {
            get { return _data; }
        }

        /// <inheritdoc />
        public CertificateHostAlgorithm(string name, int priority, Key key, byte[] data, int maxKeyFields) 
            : base(name, priority, key, data, maxKeyFields)
        {
            _data = data;
        }

        /// <inheritdoc />
        public CertificateHostAlgorithm(string name, Key key)
            : base(name, key)
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
