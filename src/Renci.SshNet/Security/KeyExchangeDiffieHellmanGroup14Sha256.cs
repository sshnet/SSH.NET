﻿using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group14-sha256" algorithm implementation.
    /// </summary>
    internal sealed class KeyExchangeDiffieHellmanGroup14Sha256 : KeyExchangeDiffieHellmanGroupSha256
    {
        /// <summary>
        /// Defined in https://tools.ietf.org/html/rfc2409#section-6.2.
        /// </summary>
        private static readonly byte[] SecondOkleyGroupReversed =
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x68, 0xaa, 0xac, 0x8a,
                0x5a, 0x8e, 0x72, 0x15, 0x10, 0x05, 0xfa, 0x98, 0x18, 0x26, 0xd2, 0x15,
                0xe5, 0x6a, 0x95, 0xea, 0x7c, 0x49, 0x95, 0x39, 0x18, 0x17, 0x58, 0x95,
                0xf6, 0xcb, 0x2b, 0xde, 0xc9, 0x52, 0x4c, 0x6f, 0xf0, 0x5d, 0xc5, 0xb5,
                0x8f, 0xa2, 0x07, 0xec, 0xa2, 0x83, 0x27, 0x9b, 0x03, 0x86, 0x0e, 0x18,
                0x2c, 0x77, 0x9e, 0xe3, 0x3b, 0xce, 0x36, 0x2e, 0x46, 0x5e, 0x90, 0x32,
                0x7c, 0x21, 0x18, 0xca, 0x08, 0x6c, 0x74, 0xf1, 0x04, 0x98, 0xbc, 0x4a,
                0x4e, 0x35, 0x0c, 0x67, 0x6d, 0x96, 0x96, 0x70, 0x07, 0x29, 0xd5, 0x9e,
                0xbb, 0x52, 0x85, 0x20, 0x56, 0xf3, 0x62, 0x1c, 0x96, 0xad, 0xa3, 0xdc,
                0x23, 0x5d, 0x65, 0x83, 0x5f, 0xcf, 0x24, 0xfd, 0xa8, 0x3f, 0x16, 0x69,
                0x9a, 0xd3, 0x55, 0x1c, 0x36, 0x48, 0xda, 0x98, 0x05, 0xbf, 0x63, 0xa1,
                0xb8, 0x7c, 0x00, 0xc2, 0x3d, 0x5b, 0xe4, 0xec, 0x51, 0x66, 0x28, 0x49,
                0xe6, 0x1f, 0x4b, 0x7c, 0x11, 0x24, 0x9f, 0xae, 0xa5, 0x9f, 0x89, 0x5a,
                0xfb, 0x6b, 0x38, 0xee, 0xed, 0xb7, 0x06, 0xf4, 0xb6, 0x5c, 0xff, 0x0b,
                0x6b, 0xed, 0x37, 0xa6, 0xe9, 0x42, 0x4c, 0xf4, 0xc6, 0x7e, 0x5e, 0x62,
                0x76, 0xb5, 0x85, 0xe4, 0x45, 0xc2, 0x51, 0x6d, 0x6d, 0x35, 0xe1, 0x4f,
                0x37, 0x14, 0x5f, 0xf2, 0x6d, 0x0a, 0x2b, 0x30, 0x1b, 0x43, 0x3a, 0xcd,
                0xb3, 0x19, 0x95, 0xef, 0xdd, 0x04, 0x34, 0x8e, 0x79, 0x08, 0x4a, 0x51,
                0x22, 0x9b, 0x13, 0x3b, 0xa6, 0xbe, 0x0b, 0x02, 0x74, 0xcc, 0x67, 0x8a,
                0x08, 0x4e, 0x02, 0x29, 0xd1, 0x1c, 0xdc, 0x80, 0x8b, 0x62, 0xc6, 0xc4,
                0x34, 0xc2, 0x68, 0x21, 0xa2, 0xda, 0x0f, 0xc9, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff,
                0x00
            };

        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "diffie-hellman-group14-sha256"; }
        }

        /// <summary>
        /// Gets the group prime.
        /// </summary>
        /// <value>
        /// The group prime.
        /// </value>
        public override BigInteger GroupPrime
        {
            get
            {
                return new BigInteger(SecondOkleyGroupReversed);
            }
        }
    }
}
