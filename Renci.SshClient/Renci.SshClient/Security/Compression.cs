using System.Collections.Generic;
namespace Renci.SshClient.Security
{
    internal abstract class Compression : Algorithm
    {
        protected Session Session { get; private set; }

        public Compression(Session session)
        {
            this.Session = session;
        }

        public abstract IEnumerable<byte> Compress(IEnumerable<byte> data);

        public abstract IEnumerable<byte> Uncompress(IEnumerable<byte> data);
    }
}
