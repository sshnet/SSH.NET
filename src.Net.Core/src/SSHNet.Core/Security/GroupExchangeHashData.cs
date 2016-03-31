﻿using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    internal class GroupExchangeHashData : SshData
    {
#if true //old TUNING
        private byte[] _serverVersion;
        private byte[] _clientVersion;
        private byte[] _prime;
        private byte[] _subGroup;
        private byte[] _clientExchangeValue;
        private byte[] _serverExchangeValue;
        private byte[] _sharedKey;
#endif

#if true //old TUNING
        public string ServerVersion
        {
            private get { return Utf8.GetString(_serverVersion, 0, _serverVersion.Length); }
            set { _serverVersion = Utf8.GetBytes(value); }
        }
#else
        public string ServerVersion { get; set; }
#endif

#if true //old TUNING
        public string ClientVersion
        {
            private get { return Utf8.GetString(_clientVersion, 0, _clientVersion.Length); }
            set { _clientVersion = Utf8.GetBytes(value); }
        }
#else
        public string ClientVersion { get; set; }
#endif

        public byte[] ClientPayload { get; set; }

        public byte[] ServerPayload { get; set; }

        public byte[] HostKey { get; set; }

        public UInt32 MinimumGroupSize { get; set; }

        public UInt32 PreferredGroupSize { get; set; }

        public UInt32 MaximumGroupSize { get; set; }

#if true //old TUNING
        public BigInteger Prime
        {
            private get { return _prime.ToBigInteger(); }
            set { _prime = value.ToByteArray().Reverse(); }
        }
#else
        public BigInteger Prime { get; set; }
#endif

#if true //old TUNING
        public BigInteger SubGroup
        {
            private get { return _subGroup.ToBigInteger(); }
            set { _subGroup = value.ToByteArray().Reverse(); }
        }
#else
        public BigInteger SubGroup { get; set; }
#endif

#if true //old TUNING
        public BigInteger ClientExchangeValue
        {
            private get { return _clientExchangeValue.ToBigInteger(); }
            set { _clientExchangeValue = value.ToByteArray().Reverse(); }
        }
#else
        public BigInteger ClientExchangeValue { get; set; }
#endif

#if true //old TUNING
        public BigInteger ServerExchangeValue
        {
            private get { return _serverExchangeValue.ToBigInteger(); }
            set { _serverExchangeValue = value.ToByteArray().Reverse(); }
        }
#else
        public BigInteger ServerExchangeValue { get; set; }
#endif

#if true //old TUNING
        public BigInteger SharedKey
        {
            private get { return _sharedKey.ToBigInteger(); }
            set { _sharedKey = value.ToByteArray().Reverse(); }
        }
#else
        public BigInteger SharedKey { get; set; }
#endif

#if true //old TUNING
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
                capacity += _clientExchangeValue.Length; // ClientExchangeValue
                capacity += 4; // ServerExchangeValue length
                capacity += _serverExchangeValue.Length; // ServerExchangeValue
                capacity += 4; // SharedKey length
                capacity += _sharedKey.Length; // SharedKey
                return capacity;
            }
        }
#endif

        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        protected override void SaveData()
        {
#if true //old TUNING
            WriteBinaryString(_clientVersion);
            WriteBinaryString(_serverVersion);
#else
            this.Write(this.ClientVersion);
            this.Write(this.ServerVersion);
#endif
            this.WriteBinaryString(this.ClientPayload);
            this.WriteBinaryString(this.ServerPayload);
            this.WriteBinaryString(this.HostKey);
            this.Write(this.MinimumGroupSize);
            this.Write(this.PreferredGroupSize);
            this.Write(this.MaximumGroupSize);
#if true //old TUNING
            WriteBinaryString(_prime);
            WriteBinaryString(_subGroup);
            WriteBinaryString(_clientExchangeValue);
            WriteBinaryString(_serverExchangeValue);
            WriteBinaryString(_sharedKey);
#else
            this.Write(this.Prime);
            this.Write(this.SubGroup);
            this.Write(this.ClientExchangeValue);
            this.Write(this.ServerExchangeValue);
            this.Write(this.SharedKey);
#endif
        }
    }
}
