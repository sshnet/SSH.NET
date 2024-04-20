﻿using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    internal sealed class GroupExchangeHashData : SshData
    {
        private byte[] _serverVersion;
        private byte[] _clientVersion;
        private byte[] _prime;
        private byte[] _subGroup;

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

        public uint MinimumGroupSize { get; set; }

        public uint PreferredGroupSize { get; set; }

        public uint MaximumGroupSize { get; set; }

        public BigInteger Prime
        {
            private get { return _prime.ToBigInteger(); }
            set { _prime = value.ToByteArray().Reverse(); }
        }

        public BigInteger SubGroup
        {
            private get { return _subGroup.ToBigInteger(); }
            set { _subGroup = value.ToByteArray().Reverse(); }
        }

        public byte[] ClientExchangeValue { get; set; }

        public byte[] ServerExchangeValue { get; set; }

        public byte[] SharedKey { get; set; }

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
                capacity += 4; // MinimumGroupSize
                capacity += 4; // PreferredGroupSize
                capacity += 4; // MaximumGroupSize
                capacity += 4; // Prime length
                capacity += _prime.Length; // Prime
                capacity += 4; // SubGroup length
                capacity += _subGroup.Length; // SubGroup
                capacity += 4; // ClientExchangeValue length
                capacity += ClientExchangeValue.Length; // ClientExchangeValue
                capacity += 4; // ServerExchangeValue length
                capacity += ServerExchangeValue.Length; // ServerExchangeValue
                capacity += 4; // SharedKey length
                capacity += SharedKey.Length; // SharedKey
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
            Write(MinimumGroupSize);
            Write(PreferredGroupSize);
            Write(MaximumGroupSize);
            WriteBinaryString(_prime);
            WriteBinaryString(_subGroup);
            WriteBinaryString(ClientExchangeValue);
            WriteBinaryString(ServerExchangeValue);
            WriteBinaryString(SharedKey);
        }
    }
}
