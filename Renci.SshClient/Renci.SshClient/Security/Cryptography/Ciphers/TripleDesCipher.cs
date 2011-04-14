using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshClient.Security.Cryptography
{
    /// <summary>
    /// Represents the class for the 3DES algorithm.
    /// </summary>
    public class TripleDesCipher : DesCipher
    {
        private byte[] _pass1;
        private byte[] _pass2;

        public TripleDesCipher(byte[] key, byte[] iv)
            : base(key, iv)
        {
            this._pass1 = new byte[this.BlockSize];
            this._pass2 = new byte[this.BlockSize];
        }

        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if ((inputOffset + this.BlockSize) > inputBuffer.Length)
                throw new IndexOutOfRangeException("input buffer too short");

            if ((outputOffset + this.BlockSize) > outputBuffer.Length)
                throw new IndexOutOfRangeException("output buffer too short");

            DesCipher.DesFunc(this.EncryptionKey, inputBuffer, inputOffset, this._pass1, 0);
            DesCipher.DesFunc(this.EncryptionKey, this._pass1, 0, this._pass2, 0);
            DesCipher.DesFunc(this.EncryptionKey, this._pass2, 0, outputBuffer, outputOffset);

            return this.BlockSize;
        }

        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if ((inputOffset + this.BlockSize) > inputBuffer.Length)
                throw new IndexOutOfRangeException("input buffer too short");

            if ((outputOffset + this.BlockSize) > outputBuffer.Length)
                throw new IndexOutOfRangeException("output buffer too short");

            DesCipher.DesFunc(this.DecryptionKey, inputBuffer, inputOffset, this._pass1, 0);
            DesCipher.DesFunc(this.DecryptionKey, this._pass1, 0, this._pass2, 0);
            DesCipher.DesFunc(this.DecryptionKey, this._pass2, 0, outputBuffer, outputOffset);

            return this.BlockSize;
        }
    }
}
