using System.Numerics;

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_GROUP message.
    /// </summary>
    public class KeyExchangeDhGroupExchangeGroup : Message
    {
        internal byte[] SafePrimeBytes { get; private set; }
        internal byte[] SubGroupBytes { get; private set; }

        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_KEX_DH_GEX_GROUP";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 31;
            }
        }

        /// <summary>
        /// Gets the safe prime.
        /// </summary>
        /// <value>
        /// The safe prime.
        /// </value>
        public BigInteger SafePrime
        {
            get { return SafePrimeBytes.ToBigInteger(); }
        }

        /// <summary>
        /// Gets the generator for subgroup in GF(p).
        /// </summary>
        /// <value>
        /// The sub group.
        /// </value>
        public BigInteger SubGroup
        {
            get { return SubGroupBytes.ToBigInteger(); }
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
                capacity += SafePrimeBytes.Length; // SafePrime
                capacity += 4; // SubGroup length
                capacity += SubGroupBytes.Length; // SubGroup

                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            SafePrimeBytes = ReadBinary();
            SubGroupBytes = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(SafePrimeBytes);
            WriteBinaryString(SubGroupBytes);
        }

        internal override void Process(Session session)
        {
            session.OnKeyExchangeDhGroupExchangeGroupReceived(this);
        }
    }
}
