using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_INIT message.
    /// </summary>
    [Message("SSH_MSG_KEX_DH_GEX_INIT", 32)]
    internal class KeyExchangeDhGroupExchangeInit : Message, IKeyExchangedAllowed
    {
        /// <summary>
        /// Gets the E value.
        /// </summary>
        public BigInteger E { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDhGroupExchangeInit"/> class.
        /// </summary>
        /// <param name="clientExchangeValue">The client exchange value.</param>
        public KeyExchangeDhGroupExchangeInit(BigInteger clientExchangeValue)
        {
            this.E = clientExchangeValue;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.E = this.ReadBigInt();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write(this.E);
        }
    }
}
