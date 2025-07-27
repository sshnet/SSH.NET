using System.Collections.Generic;
using System.Numerics;
using System.Text;

using Org.BouncyCastle.Math.EC.Rfc8032;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Facilitates (de)serializing encoded public key data in the format
    /// specified by RFC 4253 section 6.6.
    /// </summary>
    /// <remarks>
    /// See https://datatracker.ietf.org/doc/html/rfc4253#section-6.6.
    /// </remarks>
    public sealed class SshKeyData : SshData
    {
        /// <summary>
        /// Gets the public key format identifier.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the public key constituents.
        /// </summary>
        public BigInteger[] Keys { get; private set; }

        /// <inheritdoc/>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // Name length
                capacity += Encoding.UTF8.GetByteCount(Name); // Name

                foreach (var key in Keys)
                {
                    capacity += 4; // Key length
                    capacity += (int)(key.GetBitLength() / 8); // Key
                }

                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshKeyData"/> class.
        /// </summary>
        /// <param name="data">The encoded public key data.</param>
        public SshKeyData(byte[] data)
        {
            Load(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshKeyData"/> class.
        /// </summary>
        /// <param name="name">The public key format identifier.</param>
        /// <param name="keys">The public key constituents.</param>
        public SshKeyData(string name, BigInteger[] keys)
        {
            Name = name;
            Keys = keys;
        }

        /// <inheritdoc/>
        protected override void LoadData()
        {
            Name = ReadString();
            var keys = new List<BigInteger>();

            while (!IsEndOfData)
            {
                keys.Add(ReadBinary().ToBigInteger2());
            }

            Keys = keys.ToArray();
        }

        /// <inheritdoc/>
        protected override void SaveData()
        {
            Write(Name);

            foreach (var key in Keys)
            {
                var keyData = key.ToByteArray(isBigEndian: true);
                if (Name == "ssh-ed25519")
                {
                    keyData = keyData.TrimLeadingZeros().Pad(Ed25519.PublicKeySize);
                }

                WriteBinaryString(keyData);
            }
        }
    }
}
