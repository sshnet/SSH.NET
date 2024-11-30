using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class DesCipher
    {
        private sealed class BouncyCastleImpl : BlockCipher
        {
            private KeyParameter _parameter;
            private DesEngine _encryptor;
            private DesEngine _decryptor;

            public BouncyCastleImpl(byte[] key, CipherMode mode, CipherPadding padding)
                : base(key, 8, mode, padding)
            {
            }

            public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                if (_encryptor == null)
                {
                    _parameter ??= new KeyParameter(Key);
                    _encryptor = new DesEngine();
                    _encryptor.Init(forEncryption: true, _parameter);
                }

                return _encryptor.ProcessBlock(inputBuffer, inputOffset, outputBuffer, outputOffset);
            }

            public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                if (_decryptor == null)
                {
                    _parameter ??= new KeyParameter(Key);
                    _decryptor = new DesEngine();
                    _decryptor.Init(forEncryption: false, _parameter);
                }

                return _decryptor.ProcessBlock(inputBuffer, inputOffset, outputBuffer, outputOffset);
            }
        }
    }
}
