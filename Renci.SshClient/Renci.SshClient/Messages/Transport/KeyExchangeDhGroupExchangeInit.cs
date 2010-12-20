using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    internal class KeyExchangeDhGroupExchangeInit : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.KeyExchangeDhGroupExchangeInit; }
        }

        public BigInteger E { get; set; }

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
