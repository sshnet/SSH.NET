using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Base class for "diffie-hellman-group-exchange" algorithms.
    /// </summary>
    internal abstract class KeyExchangeDiffieHellmanGroupExchangeShaBase : KeyExchangeDiffieHellman
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
            var groupExchangeHashData = new GroupExchangeHashData
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
                };

            return Hash(groupExchangeHashData.GetBytes());
        }

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            // Register SSH_MSG_KEX_DH_GEX_GROUP message
            Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");
            // Subscribe to KeyExchangeDhGroupExchangeGroupReceived events
            Session.KeyExchangeDhGroupExchangeGroupReceived += Session_KeyExchangeDhGroupExchangeGroupReceived;

            // 1. client sends SSH_MSG_KEY_DH_GEX_REQUEST
            SendMessage(new KeyExchangeDhGroupExchangeRequest(MinimumGroupSize, PreferredGroupSize, MaximumProupSize));
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public override void Finish()
        {
            base.Finish();

            Session.KeyExchangeDhGroupExchangeGroupReceived -= Session_KeyExchangeDhGroupExchangeGroupReceived;
            Session.KeyExchangeDhGroupExchangeReplyReceived -= Session_KeyExchangeDhGroupExchangeReplyReceived;
        }

        private void Session_KeyExchangeDhGroupExchangeGroupReceived(object sender, MessageEventArgs<KeyExchangeDhGroupExchangeGroup> e)
        {
            // 2. server sends SSH_MSG_KEX_DH_GEX_GROUP
            var groupMessage = e.Message;

            // Unregister SSH_MSG_KEX_DH_GEX_GROUP message once received
            Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");
            // Unsubscribe from KeyExchangeDhGroupExchangeGroupReceived events
            Session.KeyExchangeDhGroupExchangeGroupReceived -= Session_KeyExchangeDhGroupExchangeGroupReceived;

            // Register in order to be able to receive SSH_MSG_KEX_DH_GEX_REPLY message
            Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");
            // Subscribe to KeyExchangeDhGroupExchangeReplyReceived events
            Session.KeyExchangeDhGroupExchangeReplyReceived += Session_KeyExchangeDhGroupExchangeReplyReceived;

            _prime = groupMessage.SafePrime;
            _group = groupMessage.SubGroup;

            PopulateClientExchangeValue();

            // 3. client sends SSH_MSG_KEX_DH_GEX_INIT
            SendMessage(new KeyExchangeDhGroupExchangeInit(_clientExchangeValue));
        }

        private void Session_KeyExchangeDhGroupExchangeReplyReceived(object sender, MessageEventArgs<KeyExchangeDhGroupExchangeReply> e)
        {
            // 4. server sends SSH_MSG_KEX_DH_GEX_REPLY
            var replyMessage = e.Message;

            // Unregister SSH_MSG_KEX_DH_GEX_REPLY message once received
            Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");
            // Unsubscribe from KeyExchangeDhGroupExchangeReplyReceived events
            Session.KeyExchangeDhGroupExchangeReplyReceived -= Session_KeyExchangeDhGroupExchangeReplyReceived;

            HandleServerDhReply(replyMessage.HostKey, replyMessage.F, replyMessage.Signature);

            // When SSH_MSG_KEX_DH_GEX_REPLY received key exchange is completed
            Finish();
        }
    }
}
