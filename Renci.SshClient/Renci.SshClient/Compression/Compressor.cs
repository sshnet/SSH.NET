using System.Collections.Generic;
using Renci.SshClient.Security;
namespace Renci.SshClient.Compression
{
    /// <summary>
    /// Represents base class for compression algorithm implementation
    /// </summary>
    public abstract class Compressor : Algorithm
    {
        /// <summary>
        /// Gets the session.
        /// </summary>
        protected Session Session { get; private set; }

        /// <summary>
        /// Initializes the algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        public virtual void Init(Session session)
        {
            this.Session = session;
        }

        /// <summary>
        /// Compresses the specified data.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <returns>Compressed data</returns>
        public abstract IEnumerable<byte> Compress(IEnumerable<byte> data);

        /// <summary>
        /// Decompresses the specified data.
        /// </summary>
        /// <param name="data">Compressed data.</param>
        /// <returns>Decompressed data.</returns>
        public abstract IEnumerable<byte> Decompress(IEnumerable<byte> data);
    }
}
