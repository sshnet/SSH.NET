﻿using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXECDH_INIT message.
    /// </summary>
    internal sealed class KeyExchangeEcdhInitMessage : Message, IKeyExchangedAllowed
    {
        /// <inheritdoc />
        public override string MessageName
        {
            get
            {
                return "SSH_MSG_KEX_ECDH_INIT";
            }
        }

        /// <inheritdoc />
        public override byte MessageNumber
        {
            get
            {
                return 30;
            }
        }

        /// <summary>
        /// Gets the client's ephemeral contribution to the ECDH exchange, encoded as an octet string.
        /// </summary>
        public byte[] QC { get; private set; }

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
                capacity += 4; // QC length
                capacity += QC.Length; // QC
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeEcdhInitMessage"/> class.
        /// </summary>
        public KeyExchangeEcdhInitMessage(byte[] q)
        {
            QC = q;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeEcdhInitMessage"/> class.
        /// </summary>
        public KeyExchangeEcdhInitMessage(BigInteger d, BigInteger q)
        {
            var dBytes = d.ToByteArray().Reverse();
            var qBytes = q.ToByteArray().Reverse();

            var data = new byte[dBytes.Length + qBytes.Length + 1];
            data[0] = 0x04;
            Buffer.BlockCopy(dBytes, 0, data, 1, dBytes.Length);
            Buffer.BlockCopy(qBytes, 0, data, dBytes.Length + 1, qBytes.Length);
            QC = data;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            QC = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(QC);
        }

        internal override void Process(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
