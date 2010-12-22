using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Renci.SshClient.Security;
using Renci.SshClient.Compression;
namespace Renci.SshClient
{
    public abstract class ConnectionInfo
    {
        public IDictionary<string, Type> KeyExchangeAlgorithms { get; private set; }

        public IDictionary<string, Type> Encryptions { get; private set; }

        public IDictionary<string, Type> HmacAlgorithms { get; private set; }

        public IDictionary<string, Type> HostKeyAlgorithms { get; private set; }

        public IDictionary<string, Type> SupportedAuthenticationMethods { get; private set; }

        public IDictionary<string, Type> CompressionAlgorithms { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public string Username { get; private set; }

        public TimeSpan Timeout { get; set; }

        public int RetryAttempts { get; set; }

        public int MaxSessions { get; set; }

        private ConnectionInfo()
        {
            //  Set default connection values
            this.Timeout = TimeSpan.FromSeconds(30);
            this.RetryAttempts = 10;
            this.MaxSessions = 10;

            this.KeyExchangeAlgorithms = new Dictionary<string, Type>()
            {
                {"diffie-hellman-group-exchange-sha256", typeof(KeyExchangeDiffieHellmanGroupExchangeSha256)},
                {"diffie-hellman-group-exchange-sha1", typeof(KeyExchangeDiffieHellmanGroupExchangeSha1)},
                {"diffie-hellman-group14-sha1", typeof(KeyExchangeDiffieHellmanGroup14Sha1)},
                {"diffie-hellman-group1-sha1", typeof(KeyExchangeDiffieHellmanGroup1Sha1)},
            };

            this.Encryptions = new Dictionary<string, Type>()
            {
                {"3des-cbc", typeof(CipherTripleDES)},
                {"aes128-cbc", typeof(CipherAES128CBC)},
                {"aes192-cbc", typeof(CipherAES192CBC)},
                {"aes256-cbc", typeof(CipherAES256CBC)},
                //{"blowfish-cbc", typeof(...)},
                //{"twofish256-cbc", typeof(...)},
                //{"twofish-cbc", typeof(...)},
                //{"twofish192-cbc", typeof(...)},
                //{"twofish128-cbc", typeof(...)},
                //{"serpent256-cbc", typeof(...)},
                //{"serpent192-cbc", typeof(...)},
                //{"serpent128-cbc", typeof(...)},
                //{"arcfour128", typeof(...)},
                //{"arcfour256", typeof(...)},
                //{"arcfour", typeof(...)},
                //{"idea-cbc", typeof(...)},
                //{"cast128-cbc", typeof(...)},
                //{"rijndael-cbc@lysator.liu.se", typeof(...)},
                //{"aes128-ctr", typeof(...)},
                //{"aes192-ctr", typeof(...)},
                //{"aes256-ctr", typeof(...)},
            };

            this.HmacAlgorithms = new Dictionary<string, Type>()
            {
                {"hmac-md5", typeof(HMacMD5)},
                {"hmac-sha1", typeof(HMacSha1)},
                //{"umac-64@openssh.com", typeof(HMacSha1)},
                //{"hmac-ripemd160", typeof(HMacSha1)},
                //{"hmac-ripemd160@openssh.com", typeof(HMacSha1)},
                //{"hmac-md5-96", typeof(...)},
                //{"hmac-sha1-96", typeof(...)},
                //{"none", typeof(...)},
            };

            this.HostKeyAlgorithms = new Dictionary<string, Type>()
            {
                {"ssh-rsa", typeof(CryptoPublicKeyRsa)},
                {"ssh-dss", typeof(CryptoPublicKeyDss)}, 
            };

            this.SupportedAuthenticationMethods = new Dictionary<string, Type>()
            {
                {"none", typeof(UserAuthenticationNone)},
                {"publickey", typeof(UserAuthenticationPublicKey)},
                {"password", typeof(UserAuthenticationPassword)},
                {"keyboard-interactive", typeof(UserAuthenticationKeyboardInteractive)},
                //{"hostbased", typeof(...)},                
                //{"gssapi-keyex", typeof(...)},                
                //{"gssapi-with-mic", typeof(...)},
            };

            this.CompressionAlgorithms = new Dictionary<string, Type>()
            {
                {"none", null}, 
                {"zlib", typeof(Zlib)}, 
                {"zlib@openssh.com", typeof(ZlibOpenSsh)}, 
            };

        }

        protected ConnectionInfo(string host, int port, string username)
            : this()
        {
            this.Host = host;
            this.Port = port;
            this.Username = username;
        }
    }
}
