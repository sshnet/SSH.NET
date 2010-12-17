﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Transport;
using System.Globalization;

namespace Renci.SshClient.Security
{
    internal class KeyExchangeDiffieHellmanGroup14Sha1 : KeyExchangeDiffieHellman
    {
        private static object _lock = new object();

        private static BigInteger _prime;

        private static BigInteger _group = new BigInteger(new byte[] { 2 });

        protected override BigInteger Prime
        {
            get
            {
                if (_prime.IsZero)
                {
                    lock (_lock)
                    {
                        if (_prime.IsZero)
                        {
                            var secondOkleyGroup = "00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AACAA68FFFFFFFFFFFFFFFF";
                            BigInteger.TryParse(secondOkleyGroup, System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out KeyExchangeDiffieHellmanGroup14Sha1._prime);
                        }
                    }
                }

                return KeyExchangeDiffieHellmanGroup14Sha1._prime;
            }
        }

        protected override BigInteger Group
        {
            get { return KeyExchangeDiffieHellmanGroup14Sha1._group; }
        }
    }
}
