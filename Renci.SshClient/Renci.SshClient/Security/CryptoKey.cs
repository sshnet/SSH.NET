using System.Collections.Generic;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents base class for public and private keys
    /// </summary>
    public abstract class CryptoKey
    {
        /// <summary>
        /// Gets key name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Loads key specific data.
        /// </summary>
        /// <param name="data">The data.</param>
        public abstract void Load(IEnumerable<byte> data);

        /// <summary>
        /// Gets key data byte array.
        /// </summary>
        /// <returns>The data byte array.</returns>
        public abstract IEnumerable<byte> GetBytes();
    }
}
