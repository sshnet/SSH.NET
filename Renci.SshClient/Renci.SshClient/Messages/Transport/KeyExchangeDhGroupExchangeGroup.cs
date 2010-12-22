using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    public class KeyExchangeDhGroupExchangeGroup : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.KeyExchangeDhGroupExchangeGroup; }
        }

        /// <summary>
        /// Gets or sets the safe prime.
        /// </summary>
        /// <value>
        /// The safe prime.
        /// </value>
        public BigInteger SafePrime { get; private set; }

        /// <summary>
        /// Gets or sets the generator for subgroup in GF(p).
        /// </summary>
        /// <value>
        /// The sub group.
        /// </value>
        public BigInteger SubGroup { get; private set; }

        protected override void LoadData()
        {
            this.SafePrime = this.ReadBigInteger();
            this.SubGroup = this.ReadBigInteger();
        }

        protected override void SaveData()
        {
            this.Write(this.SafePrime);
            this.Write(this.SubGroup);
        }
    }
}
