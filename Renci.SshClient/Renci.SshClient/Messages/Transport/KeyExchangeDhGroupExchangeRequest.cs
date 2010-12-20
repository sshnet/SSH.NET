using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Messages.Transport
{
    internal class KeyExchangeDhGroupExchangeRequest : Message
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.KeyExchangeDhGroupExchangeRequest; }
        }

        /// <summary>
        /// Gets or sets the minimal size in bits of an acceptable group.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public UInt32 Minimum { get; set; }

        /// <summary>
        /// Gets or sets the preferred size in bits of the group the server will send.
        /// </summary>
        /// <value>
        /// The preferred.
        /// </value>
        public UInt32 Preferred { get; set; }

        /// <summary>
        /// Gets or sets the maximal size in bits of an acceptable group.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public UInt32 Maximum { get; set; }

        protected override void LoadData()
        {
            this.Minimum = this.ReadUInt32();
            this.Preferred = this.ReadUInt32();
            this.Maximum = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            this.Write(this.Minimum);
            this.Write(this.Preferred);
            this.Write(this.Maximum);
        }
    }
}
