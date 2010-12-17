using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    public abstract class CipherAES : Cipher
    {
        private SymmetricAlgorithm _algorithm;

        private ICryptoTransform _encryptor;

        private ICryptoTransform _decryptor;

        public override int KeySize
        {
            get
            {
                return this._algorithm.KeySize;
            }
        }

        public override int BlockSize
        {
            get
            {
                return this._algorithm.BlockSize / 8;
            }
        }

        protected int KeyBitsSize { get; private set; }

        public CipherAES(int keyBitsSize)
        {
            this.KeyBitsSize = keyBitsSize;
            this._algorithm = new System.Security.Cryptography.RijndaelManaged();
            this._algorithm.KeySize = keyBitsSize;
            this._algorithm.Mode = System.Security.Cryptography.CipherMode.CBC;
            this._algorithm.Padding = System.Security.Cryptography.PaddingMode.None;
        }

        public override IEnumerable<byte> Encrypt(IEnumerable<byte> data)
        {
            if (this._encryptor == null)
            {
                this._encryptor = this._algorithm.CreateEncryptor(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray());
            }

            var input = data.ToArray();
            var output = new byte[input.Length];
            var writtenBytes = this._encryptor.TransformBlock(input, 0, input.Length, output, 0);

            if (writtenBytes < input.Length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }

        public override IEnumerable<byte> Decrypt(IEnumerable<byte> data)
        {
            if (this._decryptor == null)
            {
                this._decryptor = this._algorithm.CreateDecryptor(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray());
            }

            var input = data.ToArray();
            var output = new byte[input.Length];
            var writtenBytes = this._decryptor.TransformBlock(input, 0, input.Length, output, 0);

            if (writtenBytes < input.Length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }

        private bool _isDisposed = false;

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._algorithm != null)
                    {
                        this._algorithm.Dispose();
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }
    }

    public class CipherAES128CBC : CipherAES
    {
        public override string Name
        {
            get { return "aes128-cbc"; }
        }

        public CipherAES128CBC()
            : base(128)
        {

        }
    }

    public class CipherAES192CBC : CipherAES
    {
        public override string Name
        {
            get { return "aes192-cbc"; }
        }
        
        public CipherAES192CBC()
            : base(192)
        {

        }
    }

    public class CipherAES256CBC : CipherAES
    {
        public override string Name
        {
            get { return "aes256-cbc"; }
        }
        
        public CipherAES256CBC()
            : base(256)
        {

        }
    }

}
