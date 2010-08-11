using System.Collections.Generic;
namespace Renci.SshClient.Algorithms
{
    internal abstract class Signature : Algorithm
    {
        protected IEnumerable<byte> Data { get; private set; }

        public Signature(IEnumerable<byte> data)
        {
            this.Data = data;
        }

        public abstract bool ValidateSignature(IEnumerable<byte> hash, IEnumerable<byte> signature);
    }
}
