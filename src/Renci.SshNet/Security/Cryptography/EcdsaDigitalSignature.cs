using System;
using System.Globalization;

#if !NET
using Org.BouncyCastle.Crypto.Signers;
#endif

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements ECDSA digital signature algorithm.
    /// </summary>
    public class EcdsaDigitalSignature : DigitalSignature, IDisposable
    {
        private readonly EcdsaKey _key;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaDigitalSignature" /> class.
        /// </summary>
        /// <param name="key">The ECDSA key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public EcdsaDigitalSignature(EcdsaKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _key = key;
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        /// <see langword="true"/> if signature was successfully verified; otherwise <see langword="false"/>.
        /// </returns>
        public override bool Verify(byte[] input, byte[] signature)
        {
            // for 521 sig_size is 132
            var sig_size = _key.KeyLength == 521 ? 132 : _key.KeyLength / 4;
            var ssh_data = new SshDataSignature(signature, sig_size);
#if !NET
            if (_key.PublicKeyParameters != null)
            {
                var signer = new DsaDigestSigner(new ECDsaSigner(), _key.Digest, PlainDsaEncoding.Instance);
                signer.Init(forSigning: false, _key.PublicKeyParameters);
                signer.BlockUpdate(input, 0, input.Length);

                return signer.VerifySignature(ssh_data.Signature);
            }
#endif

#if NETFRAMEWORK
            var ecdsa = _key.Ecdsa;
            ecdsa.HashAlgorithm = _key.HashAlgorithm;
            return ecdsa.VerifyData(input, ssh_data.Signature);
#else
            return _key.Ecdsa.VerifyData(input, ssh_data.Signature, _key.HashAlgorithm);
#endif
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Signed input data.
        /// </returns>
        public override byte[] Sign(byte[] input)
        {
            byte[] signed = null;

#if !NET
            if (_key.PrivateKeyParameters != null)
            {
                var signer = new DsaDigestSigner(new ECDsaSigner(), _key.Digest, PlainDsaEncoding.Instance);
                signer.Init(forSigning: true, _key.PrivateKeyParameters);
                signer.BlockUpdate(input, 0, input.Length);
                signed = signer.GenerateSignature();
            }
            else
#endif
            {
#if NETFRAMEWORK
                var ecdsa = _key.Ecdsa;
                ecdsa.HashAlgorithm = _key.HashAlgorithm;
                signed = ecdsa.SignData(input);
#else
                signed = _key.Ecdsa.SignData(input, _key.HashAlgorithm);
#endif
            }

            var ssh_data = new SshDataSignature(signed.Length) { Signature = signed };
            return ssh_data.GetBytes();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="EcdsaDigitalSignature"/> class.
        /// </summary>
        ~EcdsaDigitalSignature()
        {
            Dispose(disposing: false);
        }

        private sealed class SshDataSignature : SshData
        {
            private readonly int _signature_size;

            private byte[] _signature_r;
            private byte[] _signature_s;

            public byte[] Signature
            {
                get
                {
                    var signature = new byte[_signature_size];
                    Buffer.BlockCopy(_signature_r, 0, signature, 0, _signature_r.Length);
                    Buffer.BlockCopy(_signature_s, 0, signature, _signature_r.Length, _signature_s.Length);
                    return signature;
                }
                set
                {
                    var signed_r = new byte[_signature_size / 2];
                    Buffer.BlockCopy(value, 0, signed_r, 0, signed_r.Length);
                    _signature_r = signed_r.ToBigInteger2().ToByteArray().Reverse();

                    var signed_s = new byte[_signature_size / 2];
                    Buffer.BlockCopy(value, signed_r.Length, signed_s, 0, signed_s.Length);
                    _signature_s = signed_s.ToBigInteger2().ToByteArray().Reverse();
                }
            }

            public SshDataSignature(int sig_size)
            {
                _signature_size = sig_size;
            }

            public SshDataSignature(byte[] data, int sig_size)
            {
                _signature_size = sig_size;
                Load(data);
            }

            protected override void LoadData()
            {
                _signature_r = ReadBinary().TrimLeadingZeros().Pad(_signature_size / 2);
                _signature_s = ReadBinary().TrimLeadingZeros().Pad(_signature_size / 2);
            }

            protected override void SaveData()
            {
                WriteBinaryString(_signature_r.ToBigInteger2().ToByteArray().Reverse());
                WriteBinaryString(_signature_s.ToBigInteger2().ToByteArray().Reverse());
            }

            public new byte[] ReadBinary()
            {
                var length = ReadUInt32();

                if (length > int.MaxValue)
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
                }

                return ReadBytes((int)length);
            }

            protected override int BufferCapacity
            {
                get
                {
                    var capacity = base.BufferCapacity;
                    capacity += 4; // r length
                    capacity += _signature_r.Length; // signature r
                    capacity += 4; // s length
                    capacity += _signature_s.Length; // signature s
                    return capacity;
                }
            }
        }
    }
}
