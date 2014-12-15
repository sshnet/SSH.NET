using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_GROUP message.
    /// </summary>
    [Message("SSH_MSG_KEX_DH_GEX_GROUP", 31)]
    public class KeyExchangeDhGroupExchangeGroup : Message
    {
#if TUNING
        private byte[] _safePrime;
        private byte[] _subGroup;
#endif

        /// <summary>
        /// Gets or sets the safe prime.
        /// </summary>
        /// <value>
        /// The safe prime.
        /// </value>
#if TUNING
        public BigInteger SafePrime
        {
            get { return _safePrime.ToBigInteger(); }
        }
#else
        public BigInteger SafePrime { get; private set; }
#endif

        /// <summary>
        /// Gets or sets the generator for subgroup in GF(p).
        /// </summary>
        /// <value>
        /// The sub group.
        /// </value>
#if TUNING
        public BigInteger SubGroup
        {
            get { return _subGroup.ToBigInteger(); }
        }
#else
        public BigInteger SubGroup { get; private set; }
#endif

#if TUNING
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
#endif

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
#if TUNING
            _safePrime = ReadBinary();
            _subGroup = ReadBinary();
#else
            SafePrime = ReadBigInt();
            SubGroup = ReadBigInt();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
#if TUNING
            WriteBinaryString(_safePrime);
            WriteBinaryString(_subGroup);
#else
            Write(SafePrime);
            Write(SubGroup);
#endif
        }
    }
}
