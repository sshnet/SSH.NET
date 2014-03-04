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
        public Compressor()
        {
            this._compressorStream = new MemoryStream();
            this._decompressorStream = new MemoryStream();

            this._compressor = new ZlibStream(this._compressorStream, CompressionMode.Compress);
            this._decompressor = new ZlibStream(this._decompressorStream, CompressionMode.Decompress);
        }

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
        public virtual byte[] Compress(byte[] data)
        {
            if (!this.IsActive)
            {
                return data;
            }

            this._compressorStream.SetLength(0);

            this._compressor.Write(data, 0, data.Length);

            return this._compressorStream.ToArray();
        }

        /// <summary>
        /// Decompresses the specified data.
        /// </summary>
        /// <param name="data">Compressed data.</param>
        /// <returns>Decompressed data.</returns>
        public virtual byte[] Decompress(byte[] data)
        {
            if (!this.IsActive)
            {
                return data;
            }

            this._decompressorStream.SetLength(0);

            this._decompressor.Write(data, 0, data.Length);

            return this._decompressorStream.ToArray();
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    // Dispose managed ResourceMessages.
                    if (this._compressorStream != null)
                    {
                        this._compressorStream.Dispose();
                        this._compressorStream = null;
                    }

                    if (this._decompressorStream != null)
                    {
                        this._decompressorStream.Dispose();
                        this._decompressorStream = null;
                    }
                }

                // Note disposing has been done.
                this._isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SshCommand"/> is reclaimed by garbage collection.
        /// </summary>
        ~Compressor()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
