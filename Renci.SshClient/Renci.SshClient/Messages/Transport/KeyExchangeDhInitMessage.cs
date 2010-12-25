using System.Numerics;

namespace Renci.SshClient.Messages.Transport
{
    [Message("SSH_MSG_KEXDH_INIT", 30)]
    internal class KeyExchangeDhInitMessage : Message
    {
        public BigInteger E { get; private set; }

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
