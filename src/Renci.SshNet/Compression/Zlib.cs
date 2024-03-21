#if NET6_0_OR_GREATER
using System.IO;
using System.IO.Compression;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents "zlib" compression implementation.
    /// </summary>
    internal sealed class Zlib : Compressor
    {
        private readonly string _name;

        public Zlib(bool delayedCompression)
            : base(delayedCompression)
        {
            _name = delayedCompression ? "zlib@openssh.com" : "zlib";
        }

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return _name; }
        }

        protected override byte[] CompressCore(byte[] data, int offset, int length)
        {
            using var outputStream = new MemoryStream();
            using var zlibStream = new ZLibStream(outputStream, CompressionMode.Compress);

            zlibStream.Write(data, offset, length);

            return outputStream.ToArray();
        }

        protected override byte[] DecompressCore(byte[] data, int offset, int length)
        {
            using var inputStream = new MemoryStream(data, offset, length);
            using var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress);

            using var outputStream = new MemoryStream();
            zlibStream.CopyTo(outputStream);

            return outputStream.ToArray();
        }
    }
}
#endif
