using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    public abstract class DigitalSignature
    {
        public abstract bool VerifySignature(byte[] input, byte[] signature);

        public abstract byte[] CreateSignature(byte[] input);
    }
}
