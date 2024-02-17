using System.IO;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Implements Zlib compression algorithm.
    /// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public class ZlibStream
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
#if NET6_0_OR_GREATER
        private readonly System.IO.Compression.ZLibStream _baseStream;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ZlibStream" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mode">The mode.</param>
        public ZlibStream(Stream stream, CompressionMode mode)
        {
#if NET6_0_OR_GREATER
            switch (mode)
            {
                case CompressionMode.Compress:
                    _baseStream = new System.IO.Compression.ZLibStream(stream, System.IO.Compression.CompressionMode.Compress);
                    break;
                case CompressionMode.Decompress:
                    _baseStream = new System.IO.Compression.ZLibStream(stream, System.IO.Compression.CompressionMode.Decompress);
                    break;
                default:
                    break;
            }
#endif
        }

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
#if NET6_0_OR_GREATER
            _baseStream.Write(buffer, offset, count);
#endif
        }
    }
}
