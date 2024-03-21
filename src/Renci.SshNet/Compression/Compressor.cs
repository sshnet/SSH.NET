using System;

using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Security;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents base class for compression algorithm implementation.
    /// </summary>
    public abstract class Compressor : Algorithm
    {
        private readonly bool _delayedCompression;

        private bool _isActive;
        private Session _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="Compressor"/> class.
        /// </summary>
        /// <param name="delayedCompression">
        /// <see langword="false"/> to start compression after receiving SSH_MSG_NEWKEYS.
        /// <see langword="true"/> to delay compression util receiving SSH_MSG_USERAUTH_SUCCESS.
        /// <see href="https://www.openssh.com/txt/draft-miller-secsh-compression-delayed-00.txt"/>.
        /// </param>
        protected Compressor(bool delayedCompression)
        {
            _delayedCompression = delayedCompression;
        }

        /// <summary>
        /// Initializes the algorithm.
        /// </summary>
        /// <param name="session">The session.</param>
        public virtual void Init(Session session)
        {
            if (_delayedCompression)
            {
                _session = session;
                _session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            }
            else
            {
                _isActive = true;
            }
        }

        /// <summary>
        /// Compresses the specified data.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <returns>
        /// The compressed data.
        /// </returns>
        public byte[] Compress(byte[] data)
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
            if (!_isActive)
            {
                if (offset == 0 && length == data.Length)
                {
                    return data;
                }

                var buffer = new byte[length];
                Buffer.BlockCopy(data, offset, buffer, 0, length);
                return buffer;
            }

            return CompressCore(data, offset, length);
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
        protected abstract byte[] CompressCore(byte[] data, int offset, int length);

        /// <summary>
        /// Decompresses the specified data.
        /// </summary>
        /// <param name="data">Compressed data.</param>
        /// <returns>
        /// The decompressed data.
        /// </returns>
        public byte[] Decompress(byte[] data)
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
            if (!_isActive)
            {
                if (offset == 0 && length == data.Length)
                {
                    return data;
                }

                var buffer = new byte[length];
                Buffer.BlockCopy(data, offset, buffer, 0, length);
                return buffer;
            }

            return DecompressCore(data, offset, length);
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
        protected abstract byte[] DecompressCore(byte[] data, int offset, int length);

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            _isActive = true;
            _session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
        }
    }
}
