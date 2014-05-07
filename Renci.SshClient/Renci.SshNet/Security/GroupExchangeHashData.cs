using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    internal class GroupExchangeHashData : SshData
    {
        public string ServerVersion { get; set; }

        public string ClientVersion { get; set; }

        public byte[] ClientPayload { get; set; }

        public byte[] ServerPayload { get; set; }

        public byte[] HostKey { get; set; }

        public UInt32 MinimumGroupSize { get; set; }

        public UInt32 PreferredGroupSize { get; set; }

        public UInt32 MaximumGroupSize { get; set; }

        public BigInteger Prime { get; set; }

        public BigInteger SubGroup { get; set; }

        public BigInteger ClientExchangeValue { get; set; }

        public BigInteger ServerExchangeValue { get; set; }

        public BigInteger SharedKey { get; set; }

        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        protected override void SaveData()
        {
            this.Write(this.ClientVersion);
            this.Write(this.ServerVersion);
            this.WriteBinaryString(this.ClientPayload);
            this.WriteBinaryString(this.ServerPayload);
            this.WriteBinaryString(this.HostKey);
            this.Write(this.MinimumGroupSize);
            this.Write(this.PreferredGroupSize);
            this.Write(this.MaximumGroupSize);
            this.Write(this.Prime);
            this.Write(this.SubGroup);
            this.Write(this.ClientExchangeValue);
            this.Write(this.ServerExchangeValue);
            this.Write(this.SharedKey);
        }
    }
}
