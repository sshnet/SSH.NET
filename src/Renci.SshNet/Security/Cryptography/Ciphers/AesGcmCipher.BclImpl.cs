#if NET6_0_OR_GREATER
using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    internal partial class AesGcmCipher
    {
        private sealed class BclImpl : Impl
        {
            private readonly AesGcm _aesGcm;
            private readonly int _tagSize;

            public BclImpl(byte[] key, int tagSize)
            {
#if NET8_0_OR_GREATER
                _aesGcm = new AesGcm(key, tagSize);
#else
                _aesGcm = new AesGcm(key);
#endif
                _tagSize = tagSize;
            }

            public override void Encrypt(byte[] nonce, byte[] input, int offset, int length, int aadOffset, int aadLength, byte[] output, int outOffset)
            {
                var plainText = new Span<byte>(input, offset, length);
                var cipherText = new Span<byte>(output, outOffset, length);
                var tag = new Span<byte>(output, outOffset + length, _tagSize);
                var associatedData = new Span<byte>(input, aadOffset, aadLength);

                _aesGcm.Encrypt(nonce, plainText, cipherText, tag, associatedData);
            }

            public override void Decrypt(byte[] nonce, byte[] input, int offset, int length, int aadOffset, int aadLength, byte[] output, int outOffset)
            {
                var cipherText = new ReadOnlySpan<byte>(input, offset, length);
                var tag = new ReadOnlySpan<byte>(input, offset + length, _tagSize);
                var plainText = new Span<byte>(output, outOffset, length);
                var associatedData = new ReadOnlySpan<byte>(input, aadOffset, aadLength);

                try
                {
                    _aesGcm.Decrypt(nonce, cipherText, tag, output, associatedData);
                }
#if NET8_0_OR_GREATER
                catch (AuthenticationTagMismatchException)
#else
                catch (CryptographicException ex) when (ex.Message == "The computed authentication tag did not match the input authentication tag.")
#endif
                {
                    throw new SshConnectionException("MAC error", DisconnectReason.MacError);
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _aesGcm.Dispose();
                }
            }
        }
    }
}
#endif
