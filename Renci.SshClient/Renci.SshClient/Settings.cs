
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshClient.Algorithms;
namespace Renci.SshClient
{
    internal static class Settings
    {
        public static IDictionary<string, Func<SessionInfo, KeyExchange>> KeyExchangeAlgorithms { get; private set; }

        public static IDictionary<string, Func<SymmetricAlgorithm>> Encryptions { get; private set; }

        public static IDictionary<string, Func<IEnumerable<byte>, HMAC>> HmacAlgorithms { get; private set; }

        //public static IDictionary<string, Func<IEnumerable<byte>, IEnumerable<byte>, IEnumerable<byte>, bool>> HostKeyAlgorithms { get; private set; }
        public static IDictionary<string, Func<IEnumerable<byte>, Signature>> HostKeyAlgorithms { get; private set; }


        static Settings()
        {
            Settings.KeyExchangeAlgorithms = new Dictionary<string, Func<SessionInfo, KeyExchange>>()
            {
                {"diffie-hellman-group1-sha1", (a) => { return new KeyExchangeDiffieHellman(a);}}
                //"diffie-hellman-group-exchange-sha1"

            };

            Settings.Encryptions = new Dictionary<string, Func<SymmetricAlgorithm>>()
            {
                {"3des-cbc", () => { return new System.Security.Cryptography.TripleDESCryptoServiceProvider();}},
                //{"aes128-cbc", () => { return new System.Security.Cryptography.RijndaelManaged();}},   //  TODO:   Need to be tested, currently not working
            };


            Settings.HmacAlgorithms = new Dictionary<string, Func<IEnumerable<byte>, HMAC>>()
            {
                {"hmac-md5", (key) => { return new System.Security.Cryptography.HMACMD5(key.Take(16).ToArray());}},
                {"hmac-sha1", (key) => { return new System.Security.Cryptography.HMACSHA1(key.Take(20).ToArray());}},
            };

            //Settings.HostKeyAlgorithms = new Dictionary<string, Func<IEnumerable<byte>, IEnumerable<byte>, IEnumerable<byte>, bool>>()
            Settings.HostKeyAlgorithms = new Dictionary<string, Func<IEnumerable<byte>, Signature>>()
            {
                //{"ssh-rsa", (hash, signature, hostKeyData) => { var s = new SignatureRsa(hostKeyData); return s.Validate(hash, signature);}},
                //{"ssh-dsa", (hash, signature, hostKeyData) => { var s = new SignatureDss(hostKeyData); return s.Validate(hash, signature);}}, //  TODO:   Need to be tested
                {"ssh-rsa", (hostKeyData) => { return new SignatureRsa(hostKeyData);}},
                {"ssh-dsa", (hostKeyData) => { return new SignatureDss(hostKeyData);;}}, //  TODO:   Need to be tested
            };
        }
    }
}
