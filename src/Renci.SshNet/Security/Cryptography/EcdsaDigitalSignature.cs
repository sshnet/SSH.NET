using System;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements ECDSA digital signature algorithm.
    /// </summary>
    public class EcdsaDigitalSignature : DigitalSignature, IDisposable
    {
        private readonly EcdsaKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaDigitalSignature" /> class.
        /// </summary>
        /// <param name="key">The ECDSA key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public EcdsaDigitalSignature(EcdsaKey key)
        {
            ThrowHelper.ThrowIfNull(key);

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

            return _key._impl.Verify(input, ssh_data.Signature);
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
            var signed = _key._impl.Sign(input);

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
                    _signature_r = signed_r.ToBigInteger2().ToByteArray(isBigEndian: true);

                    var signed_s = new byte[_signature_size / 2];
                    Buffer.BlockCopy(value, signed_r.Length, signed_s, 0, signed_s.Length);
                    _signature_s = signed_s.ToBigInteger2().ToByteArray(isBigEndian: true);
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
                WriteBinaryString(_signature_r.ToBigInteger2().ToByteArray(isBigEndian: true));
                WriteBinaryString(_signature_s.ToBigInteger2().ToByteArray(isBigEndian: true));
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
