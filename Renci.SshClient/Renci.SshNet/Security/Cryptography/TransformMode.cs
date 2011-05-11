using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Specifies transformation modes
    /// </summary>
    public enum TransformMode
    {
        /// <summary>
        /// Specifies encryption mode
        /// </summary>
        Encrypt = 0,
        /// <summary>
        /// Specifies decryption mode
        /// </summary>
        Decrypt = 1
    }
}
