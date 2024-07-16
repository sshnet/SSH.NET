#if NET6_0_OR_GREATER
namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents the "zlib@openssh.com" compression algorithm.
    /// </summary>
    public class ZlibOpenSsh : Zlib
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZlibOpenSsh"/> class.
        /// </summary>
        public ZlibOpenSsh()
            : base(delayedCompression: true)
        {
        }

        /// <inheritdoc/>
        public override string Name
        {
            get { return "zlib@openssh.com"; }
        }
    }
}
#endif
