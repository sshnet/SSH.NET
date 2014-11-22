using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXDH_INIT message.
    /// </summary>
    [Message("SSH_MSG_KEXDH_INIT", 30)]
    internal class KeyExchangeDhInitMessage : Message, IKeyExchangedAllowed
    {
        /// <summary>
        /// Gets the E value.
        /// </summary>
        public BigInteger E { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDhInitMessage"/> class.
        /// </summary>
        /// <param name="clientExchangeValue">The client exchange value.</param>
        public KeyExchangeDhInitMessage(BigInteger clientExchangeValue)
        {
            E = clientExchangeValue;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            ResetReader();
            E = ReadBigInt();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            Write(E);
        }
    }
}
