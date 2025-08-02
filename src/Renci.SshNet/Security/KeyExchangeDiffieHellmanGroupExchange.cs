#nullable enable
using System;
using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Provides the implementation of "diffie-hellman-group-exchange" algorithms.
    /// </summary>
    public class KeyExchangeDiffieHellmanGroupExchange : KeyExchange
    {
        private byte[]? _clientPayload;
        private byte[]? _serverPayload;
        private byte[]? _clientExchangeValue;
        private byte[]? _serverExchangeValue;
        private byte[]? _prime;
        private byte[]? _group;
        private byte[]? _hostKey;
        private byte[]? _signature;

        /// <inheritdoc/>
        public override string Name { get; }
#if NET
        private readonly IncrementalHash _hash;
#else
        private readonly HashAlgorithm _hash;
#endif
        private readonly int _hashLengthInBits;
        private readonly uint _minimumGroupSize;
        private readonly uint _preferredGroupSize;
        private readonly uint _maximumGroupSize;

        private DHParameters? _dhParameters;
        private DHBasicAgreement? _keyAgreement;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDiffieHellmanGroupExchange"/> class.
        /// </summary>
        /// <param name="name">The name of the key exchange algorithm.</param>
        /// <param name="hashAlgorithm">The hash algorithm to be used.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <see cref="HashAlgorithmName.Name"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="hashAlgorithm"/> is not a valid hash algorithm.
        /// </exception>
        public KeyExchangeDiffieHellmanGroupExchange(
            string name,
            HashAlgorithmName hashAlgorithm)
            : this(name, hashAlgorithm, 2048, 4096, 8192)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDiffieHellmanGroupExchange"/> class.
        /// </summary>
        /// <param name="name">The name of the key exchange algorithm.</param>
        /// <param name="hashAlgorithm">The hash algorithm to be used.</param>
        /// <param name="minimumGroupSize">The minimum size in bits of an acceptable group.</param>
        /// <param name="preferredGroupSize">The preferred size in bits of an acceptable group.</param>
        /// <param name="maximumGroupSize">The maximum size in bits of an acceptable group.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <see cref="HashAlgorithmName.Name"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="hashAlgorithm"/> is not a valid hash algorithm.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="preferredGroupSize"/> is not between <paramref name="minimumGroupSize"/> and <paramref name="maximumGroupSize"/>.
        /// </exception>
        public KeyExchangeDiffieHellmanGroupExchange(
            string name,
            HashAlgorithmName hashAlgorithm,
            uint minimumGroupSize,
            uint preferredGroupSize,
            uint maximumGroupSize)
        {
            ThrowHelper.ThrowIfNull(name);
            ThrowHelper.ThrowIfNullOrEmpty(hashAlgorithm.Name, nameof(hashAlgorithm));

            if (preferredGroupSize < minimumGroupSize || preferredGroupSize > maximumGroupSize)
            {
                throw new ArgumentOutOfRangeException(nameof(preferredGroupSize));
            }

            Name = name;
            _minimumGroupSize = minimumGroupSize;
            _preferredGroupSize = preferredGroupSize;
            _maximumGroupSize = maximumGroupSize;
#if NET
            try
            {
                _hash = IncrementalHash.CreateHash(hashAlgorithm);
                _hashLengthInBits = _hash.HashLengthInBytes * 8;
            }
            catch (CryptographicException cex)
            {
                throw new ArgumentException($"Could not create {nameof(HashAlgorithm)} from `{hashAlgorithm}`.", nameof(hashAlgorithm), cex);
            }
#else
            _hash = CryptoConfig.CreateFromName(hashAlgorithm.Name) as HashAlgorithm
                ?? throw new ArgumentException($"Could not create {nameof(HashAlgorithm)} from `{hashAlgorithm}`.", nameof(hashAlgorithm));
            _hashLengthInBits = _hash.HashSize;
#endif
        }

        /// <inheritdoc/>
        protected override byte[] CalculateHash()
        {
            var groupExchangeHashData = new GroupExchangeHashData
            {
                ClientVersion = Session.ClientVersion,
                ServerVersion = Session.ServerVersion,
                ClientPayload = _clientPayload,
                ServerPayload = _serverPayload,
                HostKey = _hostKey,
                MinimumGroupSize = _minimumGroupSize,
                PreferredGroupSize = _preferredGroupSize,
                MaximumGroupSize = _maximumGroupSize,
                Prime = _prime,
                SubGroup = _group,
                ClientExchangeValue = _clientExchangeValue,
                ServerExchangeValue = _serverExchangeValue,
                SharedKey = SharedKey,
            };

            return Hash(groupExchangeHashData.GetBytes());
        }

        /// <inheritdoc/>
        public override void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            base.Start(session, message, sendClientInitMessage);

            _serverPayload = message.GetBytes();
            _clientPayload = Session.ClientInitMessage.GetBytes();

            Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");

            Session.KeyExchangeDhGroupExchangeGroupReceived += Session_KeyExchangeDhGroupExchangeGroupReceived;

            // 1. client sends SSH_MSG_KEY_DH_GEX_REQUEST
            SendMessage(new KeyExchangeDhGroupExchangeRequest(_minimumGroupSize, _preferredGroupSize, _maximumGroupSize));
        }

        private void Session_KeyExchangeDhGroupExchangeGroupReceived(object? sender, MessageEventArgs<KeyExchangeDhGroupExchangeGroup> e)
        {
            // 2. server sends SSH_MSG_KEX_DH_GEX_GROUP
            var groupMessage = e.Message;

            Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");

            Session.KeyExchangeDhGroupExchangeGroupReceived -= Session_KeyExchangeDhGroupExchangeGroupReceived;

            _prime = groupMessage.SafePrimeBytes;
            _group = groupMessage.SubGroupBytes;

            // https://datatracker.ietf.org/doc/html/rfc4419#section-6.2
            var minimumBitLength = 2 * _hashLengthInBits;

            _dhParameters = new DHParameters(
                new Org.BouncyCastle.Math.BigInteger(_prime),
                new Org.BouncyCastle.Math.BigInteger(_group),
                q: null,
                m: minimumBitLength,
                l: 0);

            var g = new DHKeyPairGenerator();
            g.Init(new DHKeyGenerationParameters(CryptoAbstraction.SecureRandom, _dhParameters));

            var aKeyPair = g.GenerateKeyPair();

            _keyAgreement = new DHBasicAgreement();
            _keyAgreement.Init(aKeyPair.Private);
            _clientExchangeValue = ((DHPublicKeyParameters)aKeyPair.Public).Y.ToByteArray();

            Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");

            Session.KeyExchangeDhGroupExchangeReplyReceived += Session_KeyExchangeDhGroupExchangeReplyReceived;

            // 3. client sends SSH_MSG_KEX_DH_GEX_INIT
            SendMessage(new KeyExchangeDhGroupExchangeInit(_clientExchangeValue));
        }

        private void Session_KeyExchangeDhGroupExchangeReplyReceived(object? sender, MessageEventArgs<KeyExchangeDhGroupExchangeReply> e)
        {
            // 4. server sends SSH_MSG_KEX_DH_GEX_REPLY
            var message = e.Message;

            Session.KeyExchangeDhGroupExchangeReplyReceived -= Session_KeyExchangeDhGroupExchangeReplyReceived;

            Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");

            Session.KeyExchangeDhGroupExchangeReplyReceived -= Session_KeyExchangeDhGroupExchangeReplyReceived;

            _serverExchangeValue = message.F;
            _hostKey = message.HostKey;
            _signature = message.Signature;

            var publicKey = new DHPublicKeyParameters(new Org.BouncyCastle.Math.BigInteger(message.F), _dhParameters);

            SharedKey = _keyAgreement!.CalculateAgreement(publicKey).ToByteArray();

            // When SSH_MSG_KEX_DH_GEX_REPLY received key exchange is completed
            Finish();
        }

        /// <inheritdoc/>
        protected override bool ValidateExchangeHash()
        {
            return ValidateExchangeHash(_hostKey, _signature);
        }

        /// <inheritdoc/>
        protected override byte[] Hash(byte[] hashData)
        {
#if NET
            _hash.AppendData(hashData);
            return _hash.GetHashAndReset();
#else
            return _hash.ComputeHash(hashData);
#endif
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hash.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
