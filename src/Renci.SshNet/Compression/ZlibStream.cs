using System.IO;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Implements Zlib compression algorithm.
    /// </summary>
    public class ZlibStream
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
    }
}
