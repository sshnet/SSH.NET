using System.IO;
#if NET6_0_OR_GREATER
using System.IO.Compression;
#else
using Org.BouncyCastle.Utilities.Zlib;
#endif

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents the "zlib" compression algorithm.
    /// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces
    public class Zlib : Compressor
#pragma warning restore CA1724 // Type names should not match namespaces
    {
#if NET6_0_OR_GREATER
        private readonly ZLibStream _compressor;
        private readonly ZLibStream _decompressor;
#else
        private readonly ZOutputStream _compressor;
        private readonly ZOutputStream _decompressor;
#endif
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

#if NET6_0_OR_GREATER
            _compressor = new ZLibStream(_compressorStream, CompressionMode.Compress);
            _decompressor = new ZLibStream(_decompressorStream, CompressionMode.Decompress);
#else
            _compressor = new ZOutputStream(_compressorStream, level: JZlib.Z_DEFAULT_COMPRESSION) { FlushMode = JZlib.Z_PARTIAL_FLUSH };
            _decompressor = new ZOutputStream(_decompressorStream) { FlushMode = JZlib.Z_PARTIAL_FLUSH };
#endif
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
#if NET6_0_OR_GREATER
            _decompressorStream.Write(data, offset, length);
            _decompressorStream.Position = 0;

            using var outputStream = new MemoryStream();
            _decompressor.CopyTo(outputStream);

            _decompressorStream.SetLength(0);

            return outputStream.ToArray();
#else
            _decompressorStream.SetLength(0);

            _decompressor.Write(data, offset, length);
            _decompressor.Flush();

            return _decompressorStream.ToArray();
#endif
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
