using System.Collections.Generic;
using Renci.SshClient.Security;
namespace Renci.SshClient.Compression
{
    internal abstract class Compressor : Algorithm
    {
        protected Session Session { get; private set; }

        public Compressor(Session session)
        {
            this.Session = session;
        }

        public abstract IEnumerable<byte> Compress(IEnumerable<byte> data);

        public abstract IEnumerable<byte> Uncompress(IEnumerable<byte> data);
    }
}
