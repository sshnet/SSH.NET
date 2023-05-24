﻿using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group-exchange-sha1" algorithm implementation.
    /// </summary>
    internal sealed class KeyExchangeDiffieHellmanGroupExchangeSha1 : KeyExchangeDiffieHellmanGroupExchangeShaBase
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "diffie-hellman-group-exchange-sha1"; }
        }

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
        /// Hashes the specified data bytes.
        /// </summary>
        /// <param name="hashData">The hash data.</param>
        /// <returns>
        /// The hash of the data.
        /// </returns>
        protected override byte[] Hash(byte[] hashData)
        {
            using (var sha1 = CryptoAbstraction.CreateSHA1())
            {
                return sha1.ComputeHash(hashData, 0, hashData.Length);
            }
        }
    }
}
