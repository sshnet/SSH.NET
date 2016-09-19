using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_GROUP message.
    /// </summary>
    [Message("SSH_MSG_KEX_DH_GEX_GROUP", MessageNumber)]
    public class KeyExchangeDhGroupExchangeGroup : Message
    {
        internal const byte MessageNumber = 31;

        private byte[] _safePrime;
        private byte[] _subGroup;

        /// <summary>
        /// Gets or sets the safe prime.
        /// </summary>
        /// <value>
        /// The safe prime.
        /// </value>
        public BigInteger SafePrime
        {
            get { return _safePrime.ToBigInteger(); }
        }

        /// <summary>
        /// Gets or sets the generator for subgroup in GF(p).
        /// </summary>
        /// <value>
        /// The sub group.
        /// </value>
        public BigInteger SubGroup
        {
            get { return _subGroup.ToBigInteger(); }
        }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // SafePrime length
                capacity += _safePrime.Length; // SafePrime
                capacity += 4; // SubGroup length
                capacity += _subGroup.Length; // SubGroup

                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            _safePrime = ReadBinary();
            _subGroup = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(_safePrime);
            WriteBinaryString(_subGroup);
        }

        internal override void Process(Session session)
        {
            session.OnKeyExchangeDhGroupExchangeGroupReceived(this);
        }
    }
}
