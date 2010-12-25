using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Renci.SshClient.Messages.Transport
{
    [Message("SSH_MSG_KEXINIT", 20)]
    public class KeyExchangeInitMessage : Message
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        public KeyExchangeInitMessage()
        {
            var cookie = new byte[16];
            _randomizer.GetBytes(cookie);
            this.Cookie = cookie;
        }

        #region Message Properties

        public IEnumerable<byte> Cookie { get; private set; }

        public IEnumerable<string> KeyExchangeAlgorithms { get; set; }

        public IEnumerable<string> ServerHostKeyAlgorithms { get; set; }

        public IEnumerable<string> EncryptionAlgorithmsClientToServer { get; set; }

        public IEnumerable<string> EncryptionAlgorithmsServerToClient { get; set; }

        public IEnumerable<string> MacAlgorithmsClientToSserver { get; set; }

        public IEnumerable<string> MacAlgorithmsServerToClient { get; set; }

        public IEnumerable<string> CompressionAlgorithmsClientToServer { get; set; }

        public IEnumerable<string> CompressionAlgorithmsServerToClient { get; set; }

        public IEnumerable<string> LanguagesClientToServer { get; set; }

        public IEnumerable<string> LanguagesServerToClient { get; set; }

        public bool FirstKexPacketFollows { get; set; }

        public UInt32 Reserved { get; set; }

        #endregion

        protected override void LoadData()
        {
            this.ResetReader();

            this.Cookie = this.ReadBytes(16);
            this.KeyExchangeAlgorithms = this.ReadNamesList();
            this.ServerHostKeyAlgorithms = this.ReadNamesList();
            this.EncryptionAlgorithmsClientToServer = this.ReadNamesList();
            this.EncryptionAlgorithmsServerToClient = this.ReadNamesList();
            this.MacAlgorithmsClientToSserver = this.ReadNamesList();
            this.MacAlgorithmsServerToClient = this.ReadNamesList();
            this.CompressionAlgorithmsClientToServer = this.ReadNamesList();
            this.CompressionAlgorithmsServerToClient = this.ReadNamesList();
            this.LanguagesClientToServer = this.ReadNamesList();
            this.LanguagesServerToClient = this.ReadNamesList();
            this.FirstKexPacketFollows = this.ReadBoolean();
            this.Reserved = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            this.Write(this.Cookie);
            this.Write(this.KeyExchangeAlgorithms);
            this.Write(this.ServerHostKeyAlgorithms);
            this.Write(this.EncryptionAlgorithmsClientToServer);
            this.Write(this.EncryptionAlgorithmsServerToClient);
            this.Write(this.MacAlgorithmsClientToSserver);
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
