using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ModeBase : CipherBase 
    {
        /// <summary>
        /// Gets the cipher.
        /// </summary>
        public CipherBase Cipher { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModeBase"/> class.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        public ModeBase(CipherBase cipher)
            : base(cipher.Key, cipher.IV)
        {
            this.Cipher = cipher;
        }
    }
}
