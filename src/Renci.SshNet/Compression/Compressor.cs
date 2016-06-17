using Renci.SshNet.Security;
using System.IO;
using System;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents base class for compression algorithm implementation
    /// </summary>
    public abstract class Compressor : Algorithm, IDisposable
    {
        private readonly ZlibStream _compressor;
        private readonly ZlibStream _decompressor;

        private MemoryStream _compressorStream;
        private MemoryStream _decompressorStream;

        /// <summary>
        /// Gets or sets a value indicating whether compression is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if compression is active; otherwise, <c>false</c>.
        /// </value>
        protected bool IsActive { get; set; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        protected Session Session { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Compressor"/> class.
        /// </summary>
        protected Compressor()
        {
            _compressorStream = new MemoryStream();
            _decompressorStream = new MemoryStream();

            _compressor = new ZlibStream(_compressorStream, CompressionMode.Compress);
            _decompressor = new ZlibStream(_decompressorStream, CompressionMode.Decompress);
        }

        /// <summary>
        /// Initializes the algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        public virtual void Init(Session session)
        {
            Session = session;
        }

        /// <summary>
        /// Compresses the specified data.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <returns>Compressed data</returns>
        public virtual byte[] Compress(byte[] data)
        {
            return Compress(data, 0, data.Length);
        }

        /// <summary>
        /// Compresses the specified data.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="data"/> at which to begin reading the data to compress. </param>
        /// <param name="length">The number of bytes to be compressed. </param>
        /// <returns>
        /// The compressed data.
        /// </returns>
        public virtual byte[] Compress(byte[] data, int offset, int length)
        {
            if (!IsActive)
            {
                if (offset == 0 && length == data.Length)
                    return data;

                var buffer = new byte[length];
                Buffer.BlockCopy(data, offset, buffer, 0, length);
                return buffer;
            }

            _compressorStream.SetLength(0);

            _compressor.Write(data, offset, length);

            return _compressorStream.ToArray();
        }

        /// <summary>
        /// Decompresses the specified data.
        /// </summary>
        /// <param name="data">Compressed data.</param>
        /// <returns>
        /// The decompressed data.
        /// </returns>
        public virtual byte[] Decompress(byte[] data)
        {
            return Decompress(data, 0, data.Length);
        }

        /// <summary>
        /// Decompresses the specified data.
        /// </summary>
        /// <param name="data">Compressed data.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="data"/> at which to begin reading the data to decompress. </param>
        /// <param name="length">The number of bytes to be read from the compressed data. </param>
        /// <returns>
        /// The decompressed data.
        /// </returns>
        public virtual byte[] Decompress(byte[] data, int offset, int length)
        {
            if (!IsActive)
            {
                if (offset == 0 && length == data.Length)
                    return data;

                var buffer = new byte[length];
                Buffer.BlockCopy(data, offset, buffer, 0, length);
                return buffer;
            }

            _decompressorStream.SetLength(0);

            _decompressor.Write(data, offset, length);

            return _decompressorStream.ToArray();
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                var compressorStream = _compressorStream;
                if (compressorStream != null)
                {
                    compressorStream.Dispose();
                    _compressorStream = null;
                }

                var decompressorStream = _decompressorStream;
                if (decompressorStream != null)
                {
                    decompressorStream.Dispose();
                    _decompressorStream = null;
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the <see cref="Compressor"/> is reclaimed
        /// by garbage collection.
        /// </summary>
        ~Compressor()
        {
            Dispose(false);
        }

        #endregion
    }
}
