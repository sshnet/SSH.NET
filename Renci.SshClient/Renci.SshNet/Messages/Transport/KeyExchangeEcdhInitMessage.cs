using Renci.SshNet.Common;
using System.Linq;
using System.Collections.Generic;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXECDH_INIT message.
    /// </summary>
    [Message("SSH_MSG_KEXECDH_INIT", 30)]
    internal class KeyExchangeEcdhInitMessage : Message, IKeyExchangedAllowed
    {
        /// <summary>
        /// Gets the client's ephemeral contribution to the ECDH exchange, encoded as an octet string
        /// </summary>
        public byte[] QC { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeEcdhInitMessage"/> class.
        /// </summary>
        public KeyExchangeEcdhInitMessage(BigInteger d, BigInteger q)
        {
            var data = new List<byte>();
            data.Add(0x04);
            data.AddRange(d.ToByteArray().Reverse());
            data.AddRange(q.ToByteArray().Reverse());
            this.QC = data.ToArray();
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.ResetReader();
            this.QC = this.ReadBinaryString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.WriteBinaryString(this.QC);
        }
    }
}
