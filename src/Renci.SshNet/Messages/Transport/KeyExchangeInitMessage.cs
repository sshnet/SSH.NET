using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEXINIT message.
    /// </summary>
    [Message("SSH_MSG_KEXINIT", 20)]
    public class KeyExchangeInitMessage : Message, IKeyExchangedAllowed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyExchangeInitMessage"/> class.
        /// </summary>
        public KeyExchangeInitMessage()
        {
            var cookie = new byte[16];
            CryptoAbstraction.GenerateRandom(cookie);
            Cookie = cookie;
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
        /// <c>true</c> if first key exchange packet follows; otherwise, <c>false</c>.
        /// </value>
        public bool FirstKexPacketFollows { get; set; }

        /// <summary>
        /// Gets or sets the reserved value.
        /// </summary>
        /// <value>
        /// The reserved value.
        /// </value>
        public uint Reserved { get; set; }

        #endregion

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// <c>-1</c> to indicate that the size of the message cannot be determined,
        /// or is too costly to calculate.
        /// </value>
        protected override int BufferCapacity
        {
            get { return -1; }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            Cookie = ReadBytes(16);
            KeyExchangeAlgorithms = ReadNamesList();
            ServerHostKeyAlgorithms = ReadNamesList();
            EncryptionAlgorithmsClientToServer = ReadNamesList();
            EncryptionAlgorithmsServerToClient = ReadNamesList();
            MacAlgorithmsClientToServer = ReadNamesList();
            MacAlgorithmsServerToClient = ReadNamesList();
            CompressionAlgorithmsClientToServer = ReadNamesList();
            CompressionAlgorithmsServerToClient = ReadNamesList();
            LanguagesClientToServer = ReadNamesList();
            LanguagesServerToClient = ReadNamesList();
            FirstKexPacketFollows = ReadBoolean();
            Reserved = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            Write(Cookie);
            Write(KeyExchangeAlgorithms);
            Write(ServerHostKeyAlgorithms);
            Write(EncryptionAlgorithmsClientToServer);
            Write(EncryptionAlgorithmsServerToClient);
            Write(MacAlgorithmsClientToServer);
            Write(MacAlgorithmsServerToClient);
            Write(CompressionAlgorithmsClientToServer);
            Write(CompressionAlgorithmsServerToClient);
            Write(LanguagesClientToServer);
            Write(LanguagesServerToClient);
            Write(FirstKexPacketFollows);
            Write(Reserved);
        }

        internal override void Process(Session session)
        {
            session.OnKeyExchangeInitReceived(this);
        }
    }
}
