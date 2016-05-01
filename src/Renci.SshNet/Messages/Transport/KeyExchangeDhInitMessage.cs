using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXDH_INIT message.
    /// </summary>
    [Message("SSH_MSG_KEXDH_INIT", 30)]
    internal class KeyExchangeDhInitMessage : Message, IKeyExchangedAllowed
    {
#if TUNING
        private byte[] _eBytes;
#endif

        /// <summary>
        /// Gets the E value.
        /// </summary>
#if TUNING
        public BigInteger E
        {
            get { return _eBytes.ToBigInteger(); }
        }
#else
        public BigInteger E { get; private set; }
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
                capacity += 4; // E length
                capacity += _eBytes.Length; // E
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDhInitMessage"/> class.
        /// </summary>
        /// <param name="clientExchangeValue">The client exchange value.</param>
        public KeyExchangeDhInitMessage(BigInteger clientExchangeValue)
        {
#if TUNING
            _eBytes = clientExchangeValue.ToByteArray().Reverse();
#else
            E = clientExchangeValue;
#endif
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            ResetReader();
#if TUNING
            _eBytes = ReadBinary();
#else
            E = ReadBigInt();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
#if TUNING
            WriteBinaryString(_eBytes);
#else
            Write(E);
#endif
        }
    }
}
