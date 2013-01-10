using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for asymmetric cipher implementations.
    /// </summary>
    public abstract class AsymmetricCipher  : Cipher
    {
        public override byte MinimumSize
        {
            get { return 0; }
        }
    }
}
