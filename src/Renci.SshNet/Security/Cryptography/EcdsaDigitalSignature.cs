#if FEATURE_ECDSA
using System;
using Renci.SshNet.Common;
using System.Globalization;

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
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        public EcdsaDigitalSignature(EcdsaKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            _key = key;
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        /// <c>true</c> if signature was successfully verified; otherwise <c>false</c>.
        /// </returns>
        public override bool Verify(byte[] input, byte[] signature)
        {
            // for 521 sig_size is 132
            var sig_size = _key.dsa.KeySize == 521 ? 132 : _key.dsa.KeySize / 4;
            var ssh_data = new SshDataSignature(signature, sig_size);
            return _key.dsa.VerifyData(input, ssh_data.Signature);
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
            var signed = _key.dsa.SignData(input);
            var ssh_data = new SshDataSignature(signed.Length);
            ssh_data.Signature = signed;
            return ssh_data.GetBytes();
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="EcdsaDigitalSignature"/> is reclaimed by garbage collection.
        /// </summary>
        ~EcdsaDigitalSignature()
        {
            Dispose(false);
        }

        #endregion
    }

    class SshDataSignature : SshData
    {
        private int _signature_size;

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
                _signature_r = BytesToSsh(signed_r);

                var signed_s = new byte[_signature_size / 2];
                Buffer.BlockCopy(value, signed_r.Length, signed_s, 0, signed_s.Length);
                _signature_s = BytesToSsh(signed_s);
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
            _signature_r = Padded(ReadBinary().TrimLeadingZeros(), _signature_size / 2);
            _signature_s = Padded(ReadBinary().TrimLeadingZeros(), _signature_size / 2);
        }

        protected override void SaveData()
        {
            WriteBinaryString(BytesToSsh(_signature_r));
            WriteBinaryString(BytesToSsh(_signature_s));
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

        // Fill Data with Leading-Zeros if neccesary
        private byte[] Padded(byte[] data, int length)
        {
            if (length <= data.Length)
                return data;
            var new_data = new byte[length];
            Buffer.BlockCopy(data, 0, new_data, new_data.Length - data.Length, data.Length);
            return new_data;
        }

        // Prepend a 0-Byte if neccesary
        private byte[] BytesToSsh(byte[] data)
        {
            if ((data[0] & (1 << 7)) != 0) {
                var buf = new byte[data.Length + 1];
                Buffer.BlockCopy(data, 0, buf, 1, data.Length);
                return buf;
            }
            return data;
        }
    }
}
#endif