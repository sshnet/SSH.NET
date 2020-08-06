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
    }
}
