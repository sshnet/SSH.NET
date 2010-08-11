using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    internal class CipherAES128 : Cipher
    {
        private SymmetricAlgorithm _algorithm;

        private ICryptoTransform _encryptor;

        private ICryptoTransform _decryptor;

        public override string Name
        {
            get { return "aes128-cbc"; }
        }

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

        public CipherAES128()
        {
            this._algorithm = new System.Security.Cryptography.RijndaelManaged();
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
    }
}
