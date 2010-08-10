using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    internal class KeyExchangeDhInitMessage : Message
    {
        public BigInteger E { get; set; }

        public override MessageTypes MessageType
        {
            get
            {
                return MessageTypes.DiffieHellmanKeyExchangeInit;
            }
        }

        protected override void LoadData()
        {
            this.ResetReader();
            this.E = this.ReadBigInteger();
        }

        protected override void SaveData()
        {
            this.Write(this.E);
        }
    }
}
