using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXINIT message.
    /// </summary>
    [Message("SSH_MSG_KEXINIT", 20)]
    public class KeyExchangeInitMessage : Message, IKeyExchangedAllowed
    {
        private static readonly RNGCryptoServiceProvider _randomizer = new RNGCryptoServiceProvider();

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeInitMessage"/> class.
        /// </summary>
        public KeyExchangeInitMessage()
        {
            var cookie = new byte[16];
            _randomizer.GetBytes(cookie);
            this.Cookie = cookie;
        }

        #region Message Properties

        /// <summary>
        /// Gets session cookie.
        /// </summary>
        public byte[] Cookie { get; private set; }

        /// <summary>
        /// Gets or sets supported key exchange algorithms.
        /// </summary>
        /// <value>
        /// Supported key exchange algorithms.
        /// </value>
        public string[] KeyExchangeAlgorithms { get; set; }

        /// <summary>
        /// Gets or sets supported server host key algorithms.
        /// </summary>
        /// <value>
        /// Supported server host key algorithms.
        /// </value>
        public string[] ServerHostKeyAlgorithms { get; set; }

        /// <summary>
        /// Gets or sets supported encryption algorithms client to server.
        /// </summary>
        /// <value>
        /// Supported encryption algorithms client to server.
        /// </value>
        public string[] EncryptionAlgorithmsClientToServer { get; set; }

        /// <summary>
        /// Gets or sets supported encryption algorithms server to client.
        /// </summary>
        /// <value>
        /// Supported encryption algorithms server to client.
        /// </value>
        public string[] EncryptionAlgorithmsServerToClient { get; set; }

        /// <summary>
        /// Gets or sets supported hash algorithms client to server.
        /// </summary>
        /// <value>
        /// Supported hash algorithms client to server.
        /// </value>
        public string[] MacAlgorithmsClientToServer { get; set; }

        /// <summary>
        /// Gets or sets supported hash algorithms server to client.
        /// </summary>
        /// <value>
        /// Supported hash algorithms server to client.
        /// </value>
        public string[] MacAlgorithmsServerToClient { get; set; }

        /// <summary>
        /// Gets or sets supported compression algorithms client to server.
        /// </summary>
        /// <value>
        /// Supported compression algorithms client to server.
        /// </value>
        public string[] CompressionAlgorithmsClientToServer { get; set; }

        /// <summary>
        /// Gets or sets supported compression algorithms server to client.
        /// </summary>
        /// <value>
        /// Supported compression algorithms server to client.
        /// </value>
        public string[] CompressionAlgorithmsServerToClient { get; set; }

        /// <summary>
        /// Gets or sets supported languages client to server.
        /// </summary>
        /// <value>
        /// Supported languages client to server.
        /// </value>
        public string[] LanguagesClientToServer { get; set; }

        /// <summary>
        /// Gets or sets supported languages server to client.
        /// </summary>
        /// <value>
        /// The languages server to client.
        /// </value>
        public string[] LanguagesServerToClient { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether first key exchange packet follows.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if first key exchange packet follows; otherwise, <c>false</c>.
        /// </value>
        public bool FirstKexPacketFollows { get; set; }

        /// <summary>
        /// Gets or sets the reserved value.
        /// </summary>
        /// <value>
        /// The reserved value.
        /// </value>
        public UInt32 Reserved { get; set; }

        #endregion

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            this.ResetReader();

            this.Cookie = this.ReadBytes(16);
            this.KeyExchangeAlgorithms = this.ReadNamesList();
            this.ServerHostKeyAlgorithms = this.ReadNamesList();
            this.EncryptionAlgorithmsClientToServer = this.ReadNamesList();
            this.EncryptionAlgorithmsServerToClient = this.ReadNamesList();
            this.MacAlgorithmsClientToServer = this.ReadNamesList();
            this.MacAlgorithmsServerToClient = this.ReadNamesList();
            this.CompressionAlgorithmsClientToServer = this.ReadNamesList();
            this.CompressionAlgorithmsServerToClient = this.ReadNamesList();
            this.LanguagesClientToServer = this.ReadNamesList();
            this.LanguagesServerToClient = this.ReadNamesList();
            this.FirstKexPacketFollows = this.ReadBoolean();
            this.Reserved = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            this.Write(this.Cookie);
            this.Write(this.KeyExchangeAlgorithms);
            this.Write(this.ServerHostKeyAlgorithms);
            this.Write(this.EncryptionAlgorithmsClientToServer);
            this.Write(this.EncryptionAlgorithmsServerToClient);
            this.Write(this.MacAlgorithmsClientToServer);
            this.Write(this.MacAlgorithmsServerToClient);
            this.Write(this.CompressionAlgorithmsClientToServer);
            this.Write(this.CompressionAlgorithmsServerToClient);
            this.Write(this.LanguagesClientToServer);
            this.Write(this.LanguagesServerToClient);
            this.Write(this.FirstKexPacketFollows);
            this.Write(this.Reserved);
        }
    }
}
