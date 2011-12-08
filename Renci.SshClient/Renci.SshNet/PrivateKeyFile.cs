using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Renci.SshNet.Security;
using System.Security.Cryptography;
using System.Security;
using Renci.SshNet.Common;
using System.Globalization;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet
{
    /// <summary>
    /// old private key information/
    /// </summary>
    public class PrivateKeyFile : IDisposable
    {
#if SILVERLIGHT
        private static Regex _privateKeyRegex = new Regex(@"^-----BEGIN (?<keyName>\w+) PRIVATE KEY-----\r?\n(Proc-Type: 4,ENCRYPTED\r?\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[A-F0-9]+)\r?\n\r?\n)?(?<data>([a-zA-Z0-9/+=]{1,64}\r?\n)+)-----END \k<keyName> PRIVATE KEY-----.*", RegexOptions.Multiline);
#else
        private static Regex _privateKeyRegex = new Regex(@"^-----BEGIN (?<keyName>\w+) PRIVATE KEY-----\r?\n(Proc-Type: 4,ENCRYPTED\r?\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[A-F0-9]+)\r?\n\r?\n)?(?<data>([a-zA-Z0-9/+=]{1,64}\r?\n)+)-----END \k<keyName> PRIVATE KEY-----.*", RegexOptions.Compiled | RegexOptions.Multiline);
#endif
        
        private Key _key;

        /// <summary>
        /// Gets the host key.
        /// </summary>
        public HostAlgorithm HostKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        public PrivateKeyFile(Stream privateKey)
        {
            this.Open(privateKey, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null or empty.</exception>
        /// <remarks>This method calls <see cref="System.IO.File.Open(string, System.IO.FileMode)"/> internally, this method does not catch exceptions from <see cref="System.IO.File.Open(string, System.IO.FileMode)"/>.</remarks>
        public PrivateKeyFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            using (var keyFile = File.Open(fileName, FileMode.Open))
            {
                this.Open(keyFile, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null or empty, or <paramref name="passPhrase"/> is null.</exception>
        /// <remarks>This method calls <see cref="System.IO.File.Open(string, System.IO.FileMode)"/> internally, this method does not catch exceptions from <see cref="System.IO.File.Open(string, System.IO.FileMode)"/>.</remarks>
        public PrivateKeyFile(string fileName, string passPhrase)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            using (var keyFile = File.Open(fileName, FileMode.Open))
            {
                this.Open(keyFile, passPhrase);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyFile"/> class.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="privateKey"/> or <paramref name="passPhrase"/> is null.</exception>
        public PrivateKeyFile(Stream privateKey, string passPhrase)
        {
            this.Open(privateKey, passPhrase);
        }

        /// <summary>
        /// Opens the specified private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "this._key disposed in Dispose(bool) method.")]
        private void Open(Stream privateKey, string passPhrase)
        {
            if (privateKey == null)
                throw new ArgumentNullException("privateKey");

            Match privateKeyMatch = null;

            using (StreamReader sr = new StreamReader(privateKey))
            {
                var text = sr.ReadToEnd();
                privateKeyMatch = _privateKeyRegex.Match(text);
            }

            if (!privateKeyMatch.Success)
            {
                throw new SshException("Invalid private key file.");
            }

            var keyName = privateKeyMatch.Result("${keyName}");
            var cipherName = privateKeyMatch.Result("${cipherName}");
            var salt = privateKeyMatch.Result("${salt}");
            var data = privateKeyMatch.Result("${data}");

            var binaryData = System.Convert.FromBase64String(data);

            byte[] decryptedData;

            if (!string.IsNullOrEmpty(cipherName) && !string.IsNullOrEmpty(salt))
            {
                if (string.IsNullOrEmpty(passPhrase))
                    throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");

                byte[] binarySalt = new byte[salt.Length / 2];
                for (int i = 0; i < binarySalt.Length; i++)
                    binarySalt[i] = Convert.ToByte(salt.Substring(i * 2, 2), 16);

                CipherInfo cipher = null;
                switch (cipherName)
                {
                    case "DES-EDE3-CBC":
                        cipher = new CipherInfo(192, (key, iv) => { return new TripleDesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()); });
                        break;
                    case "DES-EDE3-CFB":
                        cipher = new CipherInfo(192, (key, iv) => { return new TripleDesCipher(key, new CfbCipherMode(iv), new PKCS7Padding()); });
                        break;
                    case "DES-CBC":
                        cipher = new CipherInfo(64, (key, iv) => { return new DesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()); });
                        break;
                        //  TODO:   Implement more private key ciphers
                    //case "AES-128-CBC":
                    //    cipher = new CipherInfo(128, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()); });
                    //    break;
                    //case "AES-192-CBC":
                    //    cipher = new CipherInfo(192, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()); });
                    //    break;
                    //case "AES-256-CBC":
                    //    cipher = new CipherInfo(256, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), new PKCS7Padding()); });
                    //    break;
                    default:
                        throw new SshException(string.Format(CultureInfo.CurrentCulture, "Private key cipher \"{0}\" is not supported.", cipherName));
                }

                decryptedData = DecryptKey(cipher, binaryData, passPhrase, binarySalt);
            }
            else
            {
                decryptedData = binaryData;
            }

            switch (keyName)
            {
                case "RSA":
                    this._key = new RsaKey(decryptedData.ToArray());
                    this.HostKey = new KeyHostAlgorithm("ssh-rsa", this._key);
                    break;
                case "DSA":
                    this._key = new DsaKey(decryptedData.ToArray());
                    this.HostKey = new KeyHostAlgorithm("ssh-dss", this._key);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Key '{0}' is not supported.", keyName));
            }
        }

        /// <summary>
        /// Decrypts encrypted private key file data.
        /// </summary>
        /// <param name="cipherInfo">The cipher info.</param>
        /// <param name="cipherData">Encrypted data.</param>
        /// <param name="passPhrase">Decryption pass phrase.</param>
        /// <param name="binarySalt">Decryption binary salt.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="cipherInfo"/>, <paramref name="cipherData"/>, <paramref name="passPhrase"/> or <paramref name="binarySalt"/> is null.</exception>
        public static byte[] DecryptKey(CipherInfo cipherInfo, byte[] cipherData, string passPhrase, byte[] binarySalt)
        {
            if (cipherInfo == null)
                throw new ArgumentNullException("cipherInfo");

            if (cipherData == null)
                throw new ArgumentNullException("cipherData");

            if (binarySalt == null)
                throw new ArgumentNullException("binarySalt");

            List<byte> cipherKey = new List<byte>();

            using (var md5 = new MD5Hash())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(passPhrase);

                var initVector = passwordBytes.Concat(binarySalt);

                var hash = md5.ComputeHash(initVector.ToArray()).AsEnumerable();

                cipherKey.AddRange(hash);

                while (cipherKey.Count < cipherInfo.KeySize / 8)
                {
                    hash = hash.Concat(initVector);

                    hash = md5.ComputeHash(hash.ToArray());

                    cipherKey.AddRange(hash);
                }
            }

            var cipher = cipherInfo.Cipher(cipherKey.ToArray(), binarySalt);

            return cipher.Decrypt(cipherData);
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    // Dispose managed ResourceMessages.
                    if (this._key != null)
                    {
                        ((IDisposable)this._key).Dispose();
                        this._key = null;
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="BaseClient"/> is reclaimed by garbage collection.
        /// </summary>
        ~PrivateKeyFile()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
