using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class StreamCipher : SymmetricCipher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        protected StreamCipher(byte[] key)
            : base(key)
        {
        }
    }
}
