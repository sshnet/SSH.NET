﻿using System;
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

#if true //old TUNING
        private byte[] _safePrime;
        private byte[] _subGroup;
#endif

        /// <summary>
        /// Gets or sets the safe prime.
        /// </summary>
        /// <value>
        /// The safe prime.
        /// </value>
#if true //old TUNING
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
#if true //old TUNING
        public BigInteger SubGroup
        {
            get { return _subGroup.ToBigInteger(); }
        }
#else
        public BigInteger SubGroup { get; private set; }
#endif

#if true //old TUNING
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
#if true //old TUNING
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
#if true //old TUNING
            WriteBinaryString(_safePrime);
            WriteBinaryString(_subGroup);
#else
            Write(SafePrime);
            Write(SubGroup);
#endif
        }
    }
}
