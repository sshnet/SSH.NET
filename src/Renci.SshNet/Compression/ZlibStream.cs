using System.IO;

#pragma warning disable S125 // Sections of code should not be commented out
#pragma warning disable SA1005 // Single line comments should begin with single space

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Implements Zlib compression algorithm.
    /// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class ZlibStream
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        //private readonly Ionic.Zlib.ZlibStream _baseStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZlibStream" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mode">The mode.</param>
#pragma warning disable IDE0060 // Remove unused parameter
        public ZlibStream(Stream stream, CompressionMode mode)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            //switch (mode)
            //{
            //    case CompressionMode.Compress:
            //        this._baseStream = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.Default);
            //        break;
            //    case CompressionMode.Decompress:
            //        this._baseStream = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default);
            //        break;
            //    default:
            //        break;
            //}

            //this._baseStream.FlushMode = Ionic.Zlib.FlushType.Partial;
        }

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
#pragma warning disable IDE0060 // Remove unused parameter
        public void Write(byte[] buffer, int offset, int count)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            //this._baseStream.Write(buffer, offset, count);
        }
#pragma warning restore SA1005 // Single line comments should begin with single space
    }
}

#pragma warning restore S125 // Sections of code should not be commented out
