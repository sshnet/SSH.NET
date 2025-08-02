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
    /// Provides the implementation of "diffie-hellman-groupN" algorithms.
    /// </summary>
    public class KeyExchangeDiffieHellman : KeyExchange
    {
        private byte[]? _clientPayload;
        private byte[]? _serverPayload;
        private byte[]? _clientExchangeValue;
        private byte[]? _serverExchangeValue;
        private byte[]? _hostKey;
        private byte[]? _signature;

        /// <inheritdoc/>
        public override string Name { get; }

        private readonly DHParameters _dhParameters;
#if NET462
        private readonly HashAlgorithm _hash;
#else
        private readonly IncrementalHash _hash;
#endif

        private DHBasicAgreement? _keyAgreement;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeDiffieHellman"/> class.
        /// </summary>
        /// <param name="name">The name of the key exchange algorithm.</param>
        /// <param name="parameters">The Diffie-Hellman parameters to be used.</param>
        /// <param name="hashAlgorithm">The hash algorithm to be used.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/>, <paramref name="parameters"/>, or <see cref="HashAlgorithmName.Name"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="hashAlgorithm"/> is not a valid hash algorithm.
        /// </exception>
        public KeyExchangeDiffieHellman(
            string name,
            DHParameters parameters,
            HashAlgorithmName hashAlgorithm)
        {
            ThrowHelper.ThrowIfNull(name);
            ThrowHelper.ThrowIfNull(parameters);
            ThrowHelper.ThrowIfNullOrEmpty(hashAlgorithm.Name, nameof(hashAlgorithm));

            Name = name;
            _dhParameters = parameters;
#if NET462
            _hash = CryptoConfig.CreateFromName(hashAlgorithm.Name) as HashAlgorithm
                ?? throw new ArgumentException($"Could not create {nameof(HashAlgorithm)} from `{hashAlgorithm}`.", nameof(hashAlgorithm));
#else
            try
            {
                _hash = IncrementalHash.CreateHash(hashAlgorithm);
            }
            catch (CryptographicException cex)
            {
                throw new ArgumentException($"Could not create {nameof(HashAlgorithm)} from `{hashAlgorithm}`.", nameof(hashAlgorithm), cex);
            }
#endif
        }

        /// <inheritdoc/>
        public override void Start(Session session, KeyExchangeInitMessage message, bool sendClientInitMessage)
        {
            base.Start(session, message, sendClientInitMessage);

            _serverPayload = message.GetBytes();
            _clientPayload = Session.ClientInitMessage.GetBytes();

            var g = new DHKeyPairGenerator();
            g.Init(new DHKeyGenerationParameters(CryptoAbstraction.SecureRandom, _dhParameters));

            var aKeyPair = g.GenerateKeyPair();

            _keyAgreement = new DHBasicAgreement();
            _keyAgreement.Init(aKeyPair.Private);
            _clientExchangeValue = ((DHPublicKeyParameters)aKeyPair.Public).Y.ToByteArray();

            Session.RegisterMessage("SSH_MSG_KEXDH_REPLY");

            Session.KeyExchangeDhReplyMessageReceived += Session_KeyExchangeDhReplyMessageReceived;

            SendMessage(new KeyExchangeDhInitMessage(_clientExchangeValue));
        }

        /// <inheritdoc/>
        protected override byte[] CalculateHash()
        {
            var keyExchangeHashData = new KeyExchangeHashData
            {
                ClientVersion = Session.ClientVersion,
                ServerVersion = Session.ServerVersion,
                ClientPayload = _clientPayload,
                ServerPayload = _serverPayload,
                HostKey = _hostKey,
                ClientExchangeValue = _clientExchangeValue,
                ServerExchangeValue = _serverExchangeValue,
                SharedKey = SharedKey,
            };

            return Hash(keyExchangeHashData.GetBytes());
        }

        private void Session_KeyExchangeDhReplyMessageReceived(object? sender, MessageEventArgs<KeyExchangeDhReplyMessage> e)
        {
            var message = e.Message;

            Session.KeyExchangeDhReplyMessageReceived -= Session_KeyExchangeDhReplyMessageReceived;

            Session.UnRegisterMessage("SSH_MSG_KEXDH_REPLY");

            _serverExchangeValue = message.F;
            _hostKey = message.HostKey;
            _signature = message.Signature;

            var publicKey = new DHPublicKeyParameters(new Org.BouncyCastle.Math.BigInteger(message.F), _dhParameters);

            SharedKey = _keyAgreement!.CalculateAgreement(publicKey).ToByteArray();

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
#if NET462
            return _hash.ComputeHash(hashData);
#else
            _hash.AppendData(hashData);
            return _hash.GetHashAndReset();
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
