using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Renci.SshClient.Security;
using System.Security.Cryptography;
using System.Security;

namespace Renci.SshClient
{
    public class PrivateKeyFile
    {
        private static Regex _privateKeyRegex = new Regex(@"^-----BEGIN (?<keyName>\w+) PRIVATE KEY-----\r\n(Proc-Type: 4,ENCRYPTED\r\nDEK-Info: (?<cryptName>[A-Z0-9-]+),(?<salt>[A-F0-9]{16})\r\n\r\n)?(?<data>([a-zA-Z0-9/+=]{1,64}\r\n)+)-----END \k<keyName> PRIVATE KEY-----.*", RegexOptions.Compiled | RegexOptions.Multiline);

        private CryptoPrivateKey _key;

        public string AlgorithmName
        {
            get
            {
                return this._key.Name;
            }
        }

        public IEnumerable<byte> PublicKey
        {
            get
            {
                return this._key.GetPublicKey().GetBytes();
            }
        }

        public IEnumerable<byte> GetSignature(IEnumerable<byte> sessionId)
        {
            return this._key.GetSignature(sessionId);
        }

        public PrivateKeyFile(Stream privateKey)
        {
            this.Open(privateKey, null);
        }

        public PrivateKeyFile(string fileName)
        {
            using (var keyFile = File.Open(fileName, FileMode.Open))
            {
                this.Open(keyFile, null);
            }
        }

        public PrivateKeyFile(string fileName, string passPhrase)
        {
            using (var keyFile = File.Open(fileName, FileMode.Open))
            {
                this.Open(keyFile, passPhrase);
            }
        }

        public PrivateKeyFile(Stream privateKey, string passPhrase)
        {
            this.Open(privateKey, passPhrase);
        }

        private void Open(Stream privateKey, string passPhrase)
        {
            Match privateKeyMatch = null;

            using (StreamReader sr = new StreamReader(privateKey))
            {
                privateKeyMatch = _privateKeyRegex.Match(sr.ReadToEnd());
            }

            if (!privateKeyMatch.Success)
            {
                throw new InvalidDataException("Invalid private key file.");
            }

            var keyName = privateKeyMatch.Result("${keyName}");
            var cryptName = privateKeyMatch.Result("${cryptName}");
            var salt = privateKeyMatch.Result("${salt}");
            var data = privateKeyMatch.Result("${data}");

            var decryptedData = string.Join(string.Empty, data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            if (!string.IsNullOrEmpty(cryptName) && !string.IsNullOrEmpty(salt))
            {
                if (string.IsNullOrEmpty(passPhrase))
                    throw new InvalidOperationException("Private key is encrypted but passphrase is empty.");

                var binaryKey = Convert.FromBase64String(passPhrase);
                var binarySalt = Convert.FromBase64String(salt);

                throw new NotImplementedException();
            }

            switch (keyName)
            {
                case "RSA":
                    this._key = new CryptoPrivateKeyRsa();
                    break;
                case "DSA":
                    this._key = new CryptoPrivateKeyDss();
                    break;
                default:
                    throw new NotSupportedException(string.Format("Key '{0}' is not supported.", keyName));
            }
            var decrypted = System.Convert.FromBase64String(data);

            this._key.Load(decrypted);
        }
    }
}
