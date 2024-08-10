using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    internal partial class AesGcmCipher
    {
        private sealed class BouncyCastleImpl : Impl
        {
            private readonly KeyParameter _keyParameter;
            private readonly int _tagSize;
            private readonly GcmBlockCipher _cipher;

            public BouncyCastleImpl(byte[] key, int tagSize)
            {
                _keyParameter = new KeyParameter(key);
                _tagSize = tagSize;
                _cipher = new GcmBlockCipher(new AesEngine());
            }

            public override void Encrypt(byte[] nonce, byte[] input, int plainTextOffset, int plainTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int cipherTextOffset)
            {
                var parameters = new AeadParameters(_keyParameter, _tagSize * 8, nonce, input.Take(associatedDataOffset, associatedDataLength));
                _cipher.Init(forEncryption: true, parameters);

                var cipherTextLength = _cipher.ProcessBytes(input, plainTextOffset, plainTextLength, output, cipherTextOffset);
                _ = _cipher.DoFinal(output, cipherTextOffset + cipherTextLength);
            }

            public override void Decrypt(byte[] nonce, byte[] input, int cipherTextOffset, int cipherTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int plainTextOffset)
            {
                var parameters = new AeadParameters(_keyParameter, _tagSize * 8, nonce, input.Take(associatedDataOffset, associatedDataLength));
                _cipher.Init(forEncryption: false, parameters);

                var plainTextLength = _cipher.ProcessBytes(input, cipherTextOffset, cipherTextLength + _tagSize, output, plainTextOffset);
                try
                {
                    _ = _cipher.DoFinal(output, plainTextLength);
                }
                catch (InvalidCipherTextException ex)
                {
                    throw new SshConnectionException("MAC error", DisconnectReason.MacError, ex);
                }
            }
        }
    }
}
