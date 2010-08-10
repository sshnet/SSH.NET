using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Transport;

namespace Renci.SshClient.Algorithms
{
    internal class KeyExchangeDiffieHellman : KeyExchange
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        private static BigInteger _prime = new BigInteger(new byte[] { (byte)0x00,
											  (byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF, 
											  (byte)0xC9,(byte)0x0F,(byte)0xDA,(byte)0xA2,(byte)0x21,(byte)0x68,(byte)0xC2,(byte)0x34,
											  (byte)0xC4,(byte)0xC6,(byte)0x62,(byte)0x8B,(byte)0x80,(byte)0xDC,(byte)0x1C,(byte)0xD1,
											  (byte)0x29,(byte)0x02,(byte)0x4E,(byte)0x08,(byte)0x8A,(byte)0x67,(byte)0xCC,(byte)0x74,
											  (byte)0x02,(byte)0x0B,(byte)0xBE,(byte)0xA6,(byte)0x3B,(byte)0x13,(byte)0x9B,(byte)0x22,
											  (byte)0x51,(byte)0x4A,(byte)0x08,(byte)0x79,(byte)0x8E,(byte)0x34,(byte)0x04,(byte)0xDD,
											  (byte)0xEF,(byte)0x95,(byte)0x19,(byte)0xB3,(byte)0xCD,(byte)0x3A,(byte)0x43,(byte)0x1B,
											  (byte)0x30,(byte)0x2B,(byte)0x0A,(byte)0x6D,(byte)0xF2,(byte)0x5F,(byte)0x14,(byte)0x37,
											  (byte)0x4F,(byte)0xE1,(byte)0x35,(byte)0x6D,(byte)0x6D,(byte)0x51,(byte)0xC2,(byte)0x45,
											  (byte)0xE4,(byte)0x85,(byte)0xB5,(byte)0x76,(byte)0x62,(byte)0x5E,(byte)0x7E,(byte)0xC6,
											  (byte)0xF4,(byte)0x4C,(byte)0x42,(byte)0xE9,(byte)0xA6,(byte)0x37,(byte)0xED,(byte)0x6B,
											  (byte)0x0B,(byte)0xFF,(byte)0x5C,(byte)0xB6,(byte)0xF4,(byte)0x06,(byte)0xB7,(byte)0xED,
											  (byte)0xEE,(byte)0x38,(byte)0x6B,(byte)0xFB,(byte)0x5A,(byte)0x89,(byte)0x9F,(byte)0xA5,
											  (byte)0xAE,(byte)0x9F,(byte)0x24,(byte)0x11,(byte)0x7C,(byte)0x4B,(byte)0x1F,(byte)0xE6,
											  (byte)0x49,(byte)0x28,(byte)0x66,(byte)0x51,(byte)0xEC,(byte)0xE6,(byte)0x53,(byte)0x81,
											  (byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF,(byte)0xFF}.Reverse().ToArray());

        private static BigInteger _group = new BigInteger(new byte[] { 2 });

        private BigInteger _randomValue;

        public override string Name
        {
            get { return "diffie-hellman-group1-sha1"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDiffieHellman"/> class.
        /// </summary>
        /// <param name="sessionInfo">The session information.</param>
        internal KeyExchangeDiffieHellman(Session session)
            : base(session)
        {
        }

        public override void Start(KeyExchangeInitMessage message)
        {
            base.Start(message);

            //  TODO:   Calculate random value correctly, enforce limits
            var clientExchangeValue = BigInteger.Zero;
            while (clientExchangeValue < 1 || clientExchangeValue > ((KeyExchangeDiffieHellman._prime - 1) / 2))
            {
                this._randomValue = new BigInteger(new Random().NextDouble() * long.MaxValue);
                clientExchangeValue = System.Numerics.BigInteger.ModPow(KeyExchangeDiffieHellman._group, this._randomValue, KeyExchangeDiffieHellman._prime);
            }

            this.ServerPayload = message.GetBytes().GetSshString();

            this.ClientExchangeValue = clientExchangeValue;

            //  Register expected message replies
            Message.RegisterMessageType<KeyExchangeDhReplyMessage>(MessageTypes.KeyExchangeDhReply);

            this.SendMessage(new KeyExchangeDhInitMessage
            {
                E = this.ClientExchangeValue,
            });

            this.Session.MessageReceived += SessionInfo_MessageReceived;

        }

        public override void Finish()
        {
            base.Finish();

            this.Session.MessageReceived -= SessionInfo_MessageReceived;
        }

        private void SessionInfo_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            this.HandleMessage((dynamic)e.Message);
        }

        private void HandleMessage<T>(T message) where T : Message, new()
        {
            //  Do nothing, handle only known messages
        }

        /// <summary>
        /// Handles the KeyExchangeDhReplyMessage message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void HandleMessage(KeyExchangeDhReplyMessage message)
        {
            //  Unregister message once received
            Message.UnRegisterMessageType(MessageTypes.KeyExchangeDhReply);

            var sharedKey = System.Numerics.BigInteger.ModPow(message.F, this._randomValue, KeyExchangeDiffieHellman._prime);

            this.ServerExchangeValue = message.F;
            this.HostKey = message.HostKey;
            this.SharedKey = sharedKey;
            this.Signature = message.Signature;

            //  Validate hash value
            if (this.ValidateExchangeHash())
            {
                this.IsSuccessed = true;
                this.SendMessage(new NewKeysMessage());
            }
            else
            {
                this.IsSuccessed = false;
                this.RaiseFailed("Key negotiationed failed.");
            }
        }
    }
}
