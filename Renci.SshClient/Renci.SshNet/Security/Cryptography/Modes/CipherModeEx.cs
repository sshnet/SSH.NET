using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Provides additional cipher modes
    /// </summary>
    public enum CipherModeEx
    {
        /// <summary>
        /// Counter Block Cipher mode
        /// </summary>
        CTR = 10,   // make sure these don't overlap with ExpressionType
    }

}
