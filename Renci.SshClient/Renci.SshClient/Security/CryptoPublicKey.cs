using System.Collections.Generic;

namespace Renci.SshClient.Security
{
    public abstract class CryptoPublicKey : CryptoKey
    {
        public abstract bool VerifySignature(IEnumerable<byte> hash, IEnumerable<byte> signature);
    }
}
