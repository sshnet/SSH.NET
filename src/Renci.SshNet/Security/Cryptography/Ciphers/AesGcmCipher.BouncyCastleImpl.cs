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
            private readonly GcmBlockCipher _cipher;
            private readonly AeadParameters _parameters;

            public BouncyCastleImpl(byte[] key, byte[] nonce)
            {
                _cipher = new GcmBlockCipher(new AesEngine());
                _parameters = new AeadParameters(new KeyParameter(key), TagSizeInBytes * 8, nonce);
            }

            public override void Encrypt(byte[] input, int plainTextOffset, int plainTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int cipherTextOffset)
            {
                _cipher.Init(forEncryption: true, _parameters);
                _cipher.ProcessAadBytes(input, associatedDataOffset, associatedDataLength);
                var cipherTextLength = _cipher.ProcessBytes(input, plainTextOffset, plainTextLength, output, cipherTextOffset);
                _ = _cipher.DoFinal(output, cipherTextOffset + cipherTextLength);
            }

            public override void Decrypt(byte[] input, int cipherTextOffset, int cipherTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int plainTextOffset)
            {
                _cipher.Init(forEncryption: false, _parameters);
                _cipher.ProcessAadBytes(input, associatedDataOffset, associatedDataLength);
                var plainTextLength = _cipher.ProcessBytes(input, cipherTextOffset, cipherTextLength + TagSizeInBytes, output, plainTextOffset);
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
