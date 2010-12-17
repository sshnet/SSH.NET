using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Renci.SshClient.Security;
using System.Security.Cryptography;
using System.Security;
using Renci.SshClient.Common;

namespace Renci.SshClient
{
    public class PrivateKeyFile
    {
        private static Regex _privateKeyRegex = new Regex(@"^-----BEGIN (?<keyName>\w+) PRIVATE KEY-----\r?\n(Proc-Type: 4,ENCRYPTED\r?\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[A-F0-9]{16})\r?\n\r?\n)?(?<data>([a-zA-Z0-9/+=]{1,64}\r?\n)+)-----END \k<keyName> PRIVATE KEY-----.*", RegexOptions.Compiled | RegexOptions.Multiline);

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
                var text = sr.ReadToEnd();
                privateKeyMatch = _privateKeyRegex.Match(text);
            }

            if (!privateKeyMatch.Success)
            {
                throw new InvalidDataException("Invalid private key file.");
            }

            var keyName = privateKeyMatch.Result("${keyName}");
            var cipherName = privateKeyMatch.Result("${cipherName}");
            var salt = privateKeyMatch.Result("${salt}");
            var data = privateKeyMatch.Result("${data}");

            var binaryData = System.Convert.FromBase64String(data);

            IEnumerable<byte> decryptedData;

            if (!string.IsNullOrEmpty(cipherName) && !string.IsNullOrEmpty(salt))
            {
                if (string.IsNullOrEmpty(passPhrase))
                    throw new InvalidOperationException("Private key is encrypted but passphrase is empty.");

                byte[] binarySalt = new byte[salt.Length / 2];
                for (int i = 0; i < binarySalt.Length; i++)
                    binarySalt[i] = Convert.ToByte(salt.Substring(i * 2, 2), 16);

                switch (cipherName)
                {
                    case "DES-EDE3-CBC":
                        using (var cipher = new CipherTripleDES())
                        {
                            decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
                        }
                        break;
                    case "DES-CBC":
                        //  TODO:   Not tested
                        using (var cipher = new CipherDES())
                        {
                            decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
                        }
                        break;
                    case "AES-128-CBC":
                        //  TODO:   Not tested
                        using (var cipher = new CipherAES128CBC())
                        {
                            decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
                        }
                        break;
                    case "AES-192-CBC":
                        //  TODO:   Not tested
                        using (var cipher = new CipherAES192CBC())
                        {
                            decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
                        }
                        break;
                    case "AES-256-CBC":
                        //  TODO:   Not tested
                        using (var cipher = new CipherAES256CBC())
                        {
                            decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
                        }
                        break;
                    default:
                        throw new SshException(string.Format("Unknown private key cipher \"{0}\".", cipherName));
                }
            }
            else
            {
                decryptedData = binaryData;
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

            this._key.Load(decryptedData);
        }


        public static IEnumerable<byte> DecryptKey(Cipher cipher, byte[] cipherData, string passPhrase, byte[] binarySalt)
        {
            List<byte> cipherKey = new List<byte>();

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(passPhrase);

                var initVector = passwordBytes.Concat(binarySalt);

                var hash = md5.ComputeHash(initVector.ToArray()).AsEnumerable();

                cipherKey.AddRange(hash);

                while (cipherKey.Count < cipher.KeySize / 8)
                {
                    hash = hash.Concat(initVector);

                    hash = md5.ComputeHash(hash.ToArray());

                    cipherKey.AddRange(hash);
                }
            }

            cipher.Init(cipherKey, binarySalt);

            return cipher.Decrypt(cipherData);
        }
    }
}
