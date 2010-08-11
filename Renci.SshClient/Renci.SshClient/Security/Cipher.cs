using System.Collections.Generic;
using System.Linq;

namespace Renci.SshClient.Security
{
    internal abstract class Cipher
    {
        public abstract string Name { get; }

        public abstract int BlockSize { get; }

        public abstract int KeySize { get; }

        protected byte[] Key { get; private set; }

        protected byte[] Vector { get; private set; }

        public virtual void Init(IEnumerable<byte> key, IEnumerable<byte> vector)
        {
            this.Key = key.ToArray();
            this.Vector = vector.ToArray();
        }

        public abstract IEnumerable<byte> Encrypt(IEnumerable<byte> data);

        public abstract IEnumerable<byte> Decrypt(IEnumerable<byte> data);
    }
}
