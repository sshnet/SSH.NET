using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXDH_REPLY message.
    /// </summary>
    [Message("SSH_MSG_KEXDH_REPLY", 31)]
    public class KeyExchangeDhReplyMessage : Message
    {
#if true //old TUNING
        private byte[] _fBytes;
#endif

        /// <summary>
        /// Gets server public host key and certificates
        /// </summary>
        /// <value>The host key.</value>
        public byte[] HostKey { get; private set; }

        /// <summary>
        /// Gets the F value.
        /// </summary>
#if true //old TUNING
        public BigInteger F
        {
            get { return _fBytes.ToBigInteger(); }
        }
#else
        public BigInteger F { get; private set; }
#endif

        /// <summary>
        /// Gets the signature of H.
        /// </summary>
        /// <value>The signature.</value>
        public byte[] Signature { get; private set; }

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
                capacity += 4; // HostKey length
                capacity += HostKey.Length; // HostKey
                capacity += 4; // F length
                capacity += _fBytes.Length; // F
                capacity += 4; // Signature length
                capacity += Signature.Length; // Signature
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            ResetReader();
#if true //old TUNING
            HostKey = ReadBinary();
            _fBytes = ReadBinary();
            Signature = ReadBinary();
#else
            HostKey = ReadBinaryString();
            F = ReadBigInt();
            Signature = ReadBinaryString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(HostKey);
#if true //old TUNING
            WriteBinaryString(_fBytes);
#else
            Write(F);
#endif
            WriteBinaryString(Signature);
        }
    }
}
