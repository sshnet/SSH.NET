using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    public abstract class AsymmetricCipher
    {
        public abstract byte[] Transform(byte[] input);

        public abstract BigInteger Transform(BigInteger input);
    }
}
