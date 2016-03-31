using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    public abstract class KeyExchangeDiffieHellmanGroupExchangeShaBase : KeyExchangeDiffieHellman
    {
        private const int MinimumGroupSize = 1024;
        private const int PreferredGroupSize = 1024;
        private const int MaximumProupSize = 8192;

        /// <summary>
        /// Calculates key exchange hash value.
        /// </summary>
        /// <returns>
        /// Key exchange hash.
        /// </returns>
        protected override byte[] CalculateHash()
        {
            var hashData = new GroupExchangeHashData
                {
                    ClientVersion = Session.ClientVersion,
                    ServerVersion = Session.ServerVersion,
                    ClientPayload = _clientPayload,
                    ServerPayload = _serverPayload,
                    HostKey = _hostKey,
                    MinimumGroupSize = MinimumGroupSize,
                    PreferredGroupSize = PreferredGroupSize,
                    MaximumGroupSize = MaximumProupSize,
                    Prime = _prime,
                    SubGroup = _group,
                    ClientExchangeValue = _clientExchangeValue,
                    ServerExchangeValue = _serverExchangeValue,
                    SharedKey = SharedKey,
                }.GetBytes();

            return this.Hash(hashData);
        }

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");
            Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");

            Session.MessageReceived += Session_MessageReceived;

            //  1. send SSH_MSG_KEY_DH_GEX_REQUEST
            SendMessage(new KeyExchangeDhGroupExchangeRequest(MinimumGroupSize, PreferredGroupSize,
                MaximumProupSize));
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public override void Finish()
        {
            base.Finish();

            Session.MessageReceived -= Session_MessageReceived;
        }

        private void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            var groupMessage = e.Message as KeyExchangeDhGroupExchangeGroup;
            if (groupMessage != null)
            {
                //  Unregister message once received
                Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");

                //  2. Receive SSH_MSG_KEX_DH_GEX_GROUP
                _prime = groupMessage.SafePrime;
                _group = groupMessage.SubGroup;

                PopulateClientExchangeValue();

                //  3. Send SSH_MSG_KEX_DH_GEX_INIT
                SendMessage(new KeyExchangeDhGroupExchangeInit(_clientExchangeValue));
            }
            else
            {
                var replyMessage = e.Message as KeyExchangeDhGroupExchangeReply;
                if (replyMessage != null)
                {
                    //  Unregister message once received
                    Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");

                    HandleServerDhReply(replyMessage.HostKey, replyMessage.F, replyMessage.Signature);

                    //  When SSH_MSG_KEX_DH_GEX_REPLY received key exchange is completed
                    Finish();
                }
            }
        }
    }
}
