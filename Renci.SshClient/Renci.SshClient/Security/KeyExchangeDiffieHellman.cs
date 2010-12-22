using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Messages.Transport;
using System.Diagnostics;
using Renci.SshClient.Messages;

namespace Renci.SshClient.Security
{
    public abstract class KeyExchangeDiffieHellman : KeyExchange
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        protected BigInteger _group;

        protected BigInteger _prime;

        protected string _clientPayload;

        protected string _serverPayload;

        protected BigInteger _clientExchangeValue;

        protected BigInteger _serverExchangeValue;

        protected BigInteger _randomValue;

        protected string _hostKey;

        protected string _signature;

        protected override bool ValidateExchangeHash()
        {
            var exchangeHash = this.CalculateHash();

            var hostKey = this._hostKey;

            var signature = this._signature;

            var bytes = hostKey.GetSshBytes();

            var length = (uint)(hostKey[0] << 24 | hostKey[1] << 16 | hostKey[2] << 8 | hostKey[3]);

            var algorithmName = bytes.Skip(4).Take((int)length).GetSshString();

            var data = bytes.Skip(4 + algorithmName.Length);

            CryptoPublicKey key = this.Session.ConnectionInfo.HostKeyAlgorithms[algorithmName].CreateInstance<CryptoPublicKey>();

            key.Load(data);

            return key.VerifySignature(exchangeHash, signature.GetSshBytes());
        }

        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            this._serverPayload = message.GetBytes().GetSshString();
            this._clientPayload = this.Session.ClientInitMessage.GetBytes().GetSshString();
        }

        protected void PopulateClientExchangeValue()
        {
            if (this._group.IsZero)
                throw new ArgumentNullException("_group");

            if (this._prime.IsZero)
                throw new ArgumentNullException("_prime");

            var bytesArray = new byte[128];
            do
            {
                _randomizer.GetBytes(bytesArray);
                bytesArray[bytesArray.Length - 1] = (byte)(bytesArray[bytesArray.Length - 1] & 0x7F);   //  Ensure not a negative value
                this._randomValue = new BigInteger(bytesArray);
                this._clientExchangeValue = System.Numerics.BigInteger.ModPow(this._group, this._randomValue, this._prime);

            } while (this._clientExchangeValue < 1 || this._clientExchangeValue > ((this._prime - 1)));
        }

        protected virtual void HandleServerDhReply(string hostKey, BigInteger serverExchangeValue, string signature)
        {
            this._serverExchangeValue = serverExchangeValue;
            this._hostKey = hostKey;
            this.SharedKey = System.Numerics.BigInteger.ModPow(serverExchangeValue, this._randomValue, this._prime);
            this._signature = signature;
        }
    }
}
