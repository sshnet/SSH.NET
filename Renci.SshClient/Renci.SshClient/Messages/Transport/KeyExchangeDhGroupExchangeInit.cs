using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    [Message("SSH_MSG_KEX_DH_GEX_INIT", 32)]
    internal class KeyExchangeDhGroupExchangeInit : Message
    {
        public BigInteger E { get; private set; }

        public KeyExchangeDhGroupExchangeInit(BigInteger clientExchangeValue)
        {
            this.E = clientExchangeValue;
        }

        protected override void LoadData()
        {
            this.E = this.ReadBigInteger();
        }

        protected override void SaveData()
        {
            this.Write(this.E);
        }
    }
}
