namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group-exchange-sha1" algorithm implementation.
    /// </summary>
    public class KeyExchangeDiffieHellmanGroupExchangeSha1 : KeyExchangeDiffieHellmanGroupExchangeShaBase
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "diffie-hellman-group-exchange-sha1"; }
        }
    }
}
