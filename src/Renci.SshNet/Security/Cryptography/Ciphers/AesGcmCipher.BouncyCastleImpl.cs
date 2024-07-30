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
            private readonly byte[] _key;
            private readonly int _tagSize;
            private readonly GcmBlockCipher _cipher;

            public BouncyCastleImpl(byte[] key, int tagSize)
            {
                _key = key;
                _tagSize = tagSize;
                _cipher = new GcmBlockCipher(new AesEngine());
            }

            public override void Encrypt(byte[] nonce, byte[] input, int offset, int length, int aadOffset, int aadLength, byte[] output, int outOffset)
            {
                var parameters = new AeadParameters(new KeyParameter(_key), _tagSize * 8, nonce, input.Take(aadOffset, aadLength));
                _cipher.Init(forEncryption: true, parameters);

                var len = _cipher.ProcessBytes(input, offset, length, output, outOffset);
                _ = _cipher.DoFinal(output, outOffset + len);
            }

            public override void Decrypt(byte[] nonce, byte[] input, int offset, int length, int aadOffset, int aadLength, byte[] output, int outOffset)
            {
                var parameters = new AeadParameters(new KeyParameter(_key), _tagSize * 8, nonce, input.Take(aadOffset, aadLength));
                _cipher.Init(forEncryption: false, parameters);

                var len = _cipher.ProcessBytes(input, offset, length + _tagSize, output, outOffset);
                try
                {
                    _ = _cipher.DoFinal(output, len);
                }
                catch (InvalidCipherTextException)
                {
                    throw new SshConnectionException("MAC error", DisconnectReason.MacError);
                }
            }
        }
    }
}
