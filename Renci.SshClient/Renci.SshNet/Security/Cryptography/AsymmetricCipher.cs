using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for asymmetric cipher implementation
    /// </summary>
    public abstract class AsymmetricCipher
    {
        /// <summary>
        /// Transforms the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public abstract byte[] Transform(byte[] input);

        /// <summary>
        /// Transforms the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public abstract BigInteger Transform(BigInteger input);
    }
}
