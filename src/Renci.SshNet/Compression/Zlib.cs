#if NET6_0_OR_GREATER
using System.IO;
using System.IO.Compression;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents the "zlib" compression algorithm.
    /// </summary>
    public class Zlib : Compressor
    {
        private readonly ZLibStream _compressor;
        private readonly ZLibStream _decompressor;
        private MemoryStream _compressorStream;
        private MemoryStream _decompressorStream;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Zlib"/> class.
        /// </summary>
        public Zlib()
            : this(delayedCompression: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Zlib"/> class.
        /// </summary>
        /// <param name="delayedCompression">
        /// <inheritdoc cref="Compressor(bool)" path="/param[@name='delayedCompression']"/>
        /// </param>
        protected Zlib(bool delayedCompression)
            : base(delayedCompression)
        {
            _compressorStream = new MemoryStream();
            _decompressorStream = new MemoryStream();

            _compressor = new ZLibStream(_compressorStream, CompressionMode.Compress);
            _decompressor = new ZLibStream(_decompressorStream, CompressionMode.Decompress);
        }

        /// <inheritdoc/>
        public override string Name
        {
            get { return "zlib"; }
        }

        /// <inheritdoc/>
        protected override byte[] CompressCore(byte[] data, int offset, int length)
        {
            _compressorStream.SetLength(0);

            _compressor.Write(data, offset, length);
            _compressor.Flush();

            return _compressorStream.ToArray();
        }

        /// <inheritdoc/>
        protected override byte[] DecompressCore(byte[] data, int offset, int length)
        {
            _decompressorStream.Write(data, offset, length);
            _decompressorStream.Position = 0;

            using var outputStream = new MemoryStream();
            _decompressor.CopyTo(outputStream);

            _decompressorStream.SetLength(0);

            return outputStream.ToArray();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_isDisposed)
            {
                return;
            }

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
    }
}
#endif
