using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Renci.SshClient.Messages;

namespace Renci.SshClient.Security
{
    internal abstract class KeyExchangeAlgorithm : Algorithm
    {
        public BigInteger SharedKey { get; protected set; }

        private IEnumerable<byte> _exchangeHash;
        /// <summary>
        /// Gets the exchange hash.
        /// </summary>
        /// <value>The exchange hash.</value>
        public IEnumerable<byte> ExchangeHash
        {
            get
            {
                if (this._exchangeHash == null)
                {
                    this._exchangeHash = this.CalculateHash();
                }
                return this._exchangeHash;
            }
        }

        protected Session Session { get; set; }

        public KeyExchangeAlgorithm(Session session)
        {
            this.Session = session;
        }

        public abstract bool ValidateExchangeHash();

        public abstract void HandleMessage<T>(T message) where T : Message;

        protected abstract IEnumerable<byte> CalculateHash();

        protected IEnumerable<byte> Hash(IEnumerable<byte> hashBytes)
        {
            using (var md = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                using (var cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, md, System.Security.Cryptography.CryptoStreamMode.Write))
                {
                    var hashData = hashBytes.ToArray();
                    cs.Write(hashData, 0, hashData.Length);
                    cs.Close();
                    return md.Hash;
                }
            }
        }

        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }
    }
}
