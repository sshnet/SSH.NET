using System;
using System.Security.Cryptography;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents a key exchange algorithm.
    /// </summary>
    public interface IKeyExchange : IDisposable
    {
        /// <summary>
        /// Occurs when the host key is received.
        /// </summary>
        event EventHandler<HostKeyEventArgs> HostKeyReceived;

        /// <summary>
        /// Gets the name of the algorithm.
        /// </summary>
        /// <value>
        /// The name of the algorithm.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the exchange hash.
        /// </summary>
        /// <value>
        /// The exchange hash.
        /// </value>
        byte[] ExchangeHash { get; }

        /// <summary>
        /// Starts the key exchange algorithm.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">Key exchange init message.</param>
        void Start(Session session, KeyExchangeInitMessage message);

        /// <summary>
        /// Finishes the key exchange algorithm.
        /// </summary>
        void Finish();

        /// <summary>
        /// Creates the client-side cipher to use.
        /// </summary>
        /// <returns>
        /// The client cipher.
        /// </returns>
        Cipher CreateClientCipher();

        /// <summary>
        /// Creates the server-side cipher to use.
        /// </summary>
        /// <returns>
        /// The server cipher.
        /// </returns>
        Cipher CreateServerCipher();

        /// <summary>
        /// Creates the server-side hash algorithm to use.
        /// </summary>
        /// <returns>
        /// The server hash algorithm.
        /// </returns>
        HashAlgorithm CreateServerHash();

        /// <summary>
        /// Creates the client-side hash algorithm to use.
        /// </summary>
        /// <returns>
        /// The client hash algorithm.
        /// </returns>
        HashAlgorithm CreateClientHash();

        /// <summary>
        /// Creates the compression algorithm to use to deflate data.
        /// </summary>
        /// <returns>
        /// The compression method to deflate data.
        /// </returns>
        Compressor CreateCompressor();

        /// <summary>
        /// Creates the compression algorithm to use to inflate data.
        /// </summary>
        /// <returns>
        /// The compression method to inflate data.
        /// </returns>
        Compressor CreateDecompressor();
    }
}
