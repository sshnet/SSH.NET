namespace Renci.SshNet.Security
{
    /// <inheritdoc />
    public class CertificateKeyHostAlgorithm : KeyHostAlgorithm
    {
        private readonly byte[] _data;

        /// <inheritdoc />
        public override byte[] Data
        {
            get { return _data; }
        }

        /// <inheritdoc />
        public CertificateKeyHostAlgorithm(string name, Key key)
            : base(name, key)
        {
        }

        /// <inheritdoc />
        public CertificateKeyHostAlgorithm(string name, Key key, byte[] data, int maxKeyFields) 
            : base(name, key, data, maxKeyFields)
        {
            _data = data;
        }
    }
}