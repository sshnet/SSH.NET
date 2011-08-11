using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents RSA public key
    /// </summary>
    public class CryptoPublicKeyRsa : CryptoPublicKey
    {
        private byte[] _modulus;

        private byte[] _exponent;

        /// <summary>
        /// Gets key name.
        /// </summary>
        public override string Name
        {
            get { return "ssh-rsa"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoPublicKeyRsa"/> class.
        /// </summary>
        public CryptoPublicKeyRsa()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoPublicKeyRsa"/> class.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="exponent">The exponent.</param>
        internal CryptoPublicKeyRsa(byte[] modulus, byte[] exponent)
        {
            this._modulus = modulus;
            this._exponent = exponent;
        }

        /// <summary>
        /// Loads key specific data.
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Load(IEnumerable<byte> data)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream(data.ToArray());
                using (var br = new BinaryReader(ms))
                {
                    var el = (uint)(br.ReadByte() << 24 | br.ReadByte() << 16 | br.ReadByte() << 8 | br.ReadByte());

                    this._exponent = br.ReadBytes((int)el);

                    var ml = (uint)(br.ReadByte() << 24 | br.ReadByte() << 16 | br.ReadByte() << 8 | br.ReadByte());

                    this._modulus = br.ReadBytes((int)ml);
                }
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                    ms = null;
                }
            }
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        /// true if signature verified; otherwise false.
        /// </returns>
        public override bool VerifySignature(IEnumerable<byte> hash, IEnumerable<byte> signature)
        {
            long i = 0;
            long j = 0;
            byte[] tmp;

            var sig = signature.ToArray();
            if (sig[0] == 0 && sig[1] == 0 && sig[2] == 0)
            {
                long i1 = (sig[i++] << 24) & 0xff000000;
                long i2 = (sig[i++] << 16) & 0x00ff0000;
                long i3 = (sig[i++] << 8) & 0x0000ff00;
                long i4 = (sig[i++]) & 0x000000ff;
                j = i1 | i2 | i3 | i4;

                i += j;

                i1 = (sig[i++] << 24) & 0xff000000;
                i2 = (sig[i++] << 16) & 0x00ff0000;
                i3 = (sig[i++] << 8) & 0x0000ff00;
                i4 = (sig[i++]) & 0x000000ff;
                j = i1 | i2 | i3 | i4;

                tmp = new byte[j];
                Array.Copy(sig, (int)i, tmp, 0, (int)j);
                sig = tmp;
            }

            var sig1 = new RsaDigitalSignature(this._exponent, this._modulus);

            return sig1.VerifySignature(hash.ToArray(), sig);

        }

        /// <summary>
        /// Gets key data byte array.
        /// </summary>
        /// <returns>
        /// The data byte array.
        /// </returns>
        public override IEnumerable<byte> GetBytes()
        {
            return new RsaPublicKeyData
            {
                E = this._exponent,
                Modulus = this._modulus,
            }.GetBytes();
        }

        private class RsaPublicKeyData : SshData
        {
            public byte[] Modulus { get; set; }

            public byte[] E { get; set; }

            protected override void LoadData()
            {
            }

            protected override void SaveData()
            {
                this.Write("ssh-rsa");
                this.WriteBinaryString(this.E);
                this.WriteBinaryString(this.Modulus);
            }
        }

    }
}
