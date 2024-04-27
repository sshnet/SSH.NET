#if NET6_0_OR_GREATER
namespace Renci.SshNet.Compression
{
    internal sealed class ZlibOpenSsh : Zlib
    {
        public ZlibOpenSsh()
            : base(delayedCompression: true)
        {
        }

        public override string Name
        {
            get { return "zlib@openssh.com"; }
        }
    }
}
#endif
