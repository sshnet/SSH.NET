using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <inheritdoc />
    public class RsaCertV01Key : RsaKey
    {
        /// <inheritdoc />
        public override BigInteger[] Public
        {
            get { return base.Public; }
            set
            {
                // skip nonce
                // https://raw.githubusercontent.com/openssh/openssh-portable/master/PROTOCOL.certkeys
                _privateKey = new[] { value[2], value[1] };
            }
        }
    }
}
