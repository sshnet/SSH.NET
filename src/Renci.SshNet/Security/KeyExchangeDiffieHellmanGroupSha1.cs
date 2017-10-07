using System;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group1-sha1" algorithm implementation.
    /// </summary>
    internal abstract class KeyExchangeDiffieHellmanGroupSha1 : KeyExchangeDiffieHellman
    {
        /// <summary>
        /// Gets the group prime.
        /// </summary>
        /// <value>
        /// The group prime.
        /// </value>
        public abstract BigInteger GroupPrime { get; }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        protected override int HashSize
        {
            get { return 160; }
        }

        /// <summary>
        /// Calculates key exchange hash value.
        /// </summary>
        /// <returns>
        /// Key exchange hash.
        /// </returns>
        protected override byte[] CalculateHash()
        {
            var hashData = new _ExchangeHashData
                {
                    ClientVersion = Session.ClientVersion,
                    ServerVersion = Session.ServerVersion,
                    ClientPayload = _clientPayload,
                    ServerPayload = _serverPayload,
                    HostKey = _hostKey,
                    ClientExchangeValue = _clientExchangeValue,
                    ServerExchangeValue = _serverExchangeValue,
                    SharedKey = SharedKey,
                }.GetBytes();

            return Hash(hashData);
        }

        /// <summary>
        /// Starts key exchange algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        public override void Start(Session session, KeyExchangeInitMessage message)
        {
            base.Start(session, message);

            Session.RegisterMessage("SSH_MSG_KEXDH_REPLY");

            Session.KeyExchangeDhReplyMessageReceived += Session_KeyExchangeDhReplyMessageReceived;

            _prime = GroupPrime;
            _group = new BigInteger(new byte[] { 2 });

            PopulateClientExchangeValue();

            SendMessage(new KeyExchangeDhInitMessage(_clientExchangeValue));
        }

        /// <summary>
        /// Finishes key exchange algorithm.
        /// </summary>
        public override void Finish()
        {
            base.Finish();

            Session.KeyExchangeDhReplyMessageReceived -= Session_KeyExchangeDhReplyMessageReceived;
        }

        private void Session_KeyExchangeDhReplyMessageReceived(object sender, MessageEventArgs<KeyExchangeDhReplyMessage> e)
        {
            var message = e.Message;

            //  Unregister message once received
            Session.UnRegisterMessage("SSH_MSG_KEXDH_REPLY");

            HandleServerDhReply(message.HostKey, message.F, message.Signature);

            //  When SSH_MSG_KEXDH_REPLY received key exchange is completed
            Finish();
        }

        private class _ExchangeHashData : SshData
        {
            private byte[] _serverVersion;
            private byte[] _clientVersion;
            private byte[] _clientExchangeValue;
            private byte[] _serverExchangeValue;
            private byte[] _sharedKey;

            public string ServerVersion
            {
                private get { return Utf8.GetString(_serverVersion, 0, _serverVersion.Length); }
                set { _serverVersion = Utf8.GetBytes(value); }
            }

            public string ClientVersion
            {
                private get { return Utf8.GetString(_clientVersion, 0, _clientVersion.Length); }
                set { _clientVersion = Utf8.GetBytes(value); }
            }

            public byte[] ClientPayload { get; set; }

            public byte[] ServerPayload { get; set; }

            public byte[] HostKey { get; set; }

            public BigInteger ClientExchangeValue
            {
                private get { return _clientExchangeValue.ToBigInteger(); }
                set { _clientExchangeValue = value.ToByteArray().Reverse(); }
            }

            public BigInteger ServerExchangeValue
            {
                private get { return _serverExchangeValue.ToBigInteger(); }
                set { _serverExchangeValue = value.ToByteArray().Reverse(); }
            }

            public BigInteger SharedKey
            {
                private get { return _sharedKey.ToBigInteger(); }
                set { _sharedKey = value.ToByteArray().Reverse(); }
            }

            /// <summary>
            /// Gets the size of the message in bytes.
            /// </summary>
            /// <value>
            /// The size of the messages in bytes.
            /// </value>
            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // ClientVersion length
                    capacity += _clientVersion.Length; // ClientVersion
                    capacity += 4; // ServerVersion length
                    capacity += _serverVersion.Length; // ServerVersion
                    capacity += 4; // ClientPayload length
                    capacity += ClientPayload.Length; // ClientPayload
                    capacity += 4; // ServerPayload length
                    capacity += ServerPayload.Length; // ServerPayload
                    capacity += 4; // HostKey length
                    capacity += HostKey.Length; // HostKey
                    capacity += 4; // ClientExchangeValue length
                    capacity += _clientExchangeValue.Length; // ClientExchangeValue
                    capacity += 4; // ServerExchangeValue length
                    capacity += _serverExchangeValue.Length; // ServerExchangeValue
                    capacity += 4; // SharedKey length
                    capacity += _sharedKey.Length; // SharedKey
                    return capacity;
                }
            }

            protected override void LoadData()
            {
                throw new NotImplementedException();
            }

            protected override void SaveData()
            {
                WriteBinaryString(_clientVersion);
                WriteBinaryString(_serverVersion);
                WriteBinaryString(ClientPayload);
                WriteBinaryString(ServerPayload);
                WriteBinaryString(HostKey);
                WriteBinaryString(_clientExchangeValue);
                WriteBinaryString(_serverExchangeValue);
                WriteBinaryString(_sharedKey);
            }
        }
    }
}
