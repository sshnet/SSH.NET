using System.Collections.Generic;

namespace Renci.SshClient.Security
{
    public abstract class CryptoKey
    {
        public abstract string Name { get; }

        public abstract void Load(IEnumerable<byte> data);

        public abstract IEnumerable<byte> GetBytes();
    }
}
