using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    internal class KeyExchangeDhInitMessage : Message
    {
        public BigInteger E { get; private set; }

        public override MessageTypes MessageType
        {
            get
            {
                return MessageTypes.DiffieHellmanKeyExchangeInit;
            }
        }

        public KeyExchangeDhInitMessage(BigInteger clientExchangeValue)
        {
            this.E = clientExchangeValue;
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
