using Renci.SshNet.Common;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_GROUP message.
    /// </summary>
    public class KeyExchangeDhGroupExchangeGroupTest : TestBase
    {
        /// <summary>
        /// Gets or sets the safe prime.
        /// </summary>
        /// <value>
        /// The safe prime.
        /// </value>
        public BigInteger SafePrime { get; private set; }

        /// <summary>
        /// Gets or sets the generator for subgroup in GF(p).
        /// </summary>
        /// <value>
        /// The sub group.
        /// </value>
        public BigInteger SubGroup { get; private set; }
    }
}