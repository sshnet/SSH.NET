#nullable enable
using System;
using System.Formats.Asn1;
using System.Globalization;
using System.Numerics;

using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Pkcs;

using Renci.SshNet.Common;
using Renci.SshNet.Security;

namespace Renci.SshNet
{
    public partial class PrivateKeyFile
    {
        private sealed class PKCS8 : IPrivateKeyParser
        {
            private readonly bool _encrypted;
            private readonly byte[] _data;
            private readonly string? _passPhrase;

            public PKCS8(bool encrypted, byte[] data, string? passPhrase)
            {
                _encrypted = encrypted;
                _data = data;
                _passPhrase = passPhrase;
            }

            /// <summary>
            /// Parses an OpenSSL PKCS#8 key file according to RFC5208:
            /// <see href="https://www.rfc-editor.org/rfc/rfc5208#section-5"/>.
            /// </summary>
            /// <exception cref="SshException">Algorithm not supported.</exception>
            public Key Parse()
            {
                PrivateKeyInfo privateKeyInfo;
                if (_encrypted)
                {
                    var encryptedPrivateKeyInfo = EncryptedPrivateKeyInfo.GetInstance(_data);
                    privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(_passPhrase?.ToCharArray(), encryptedPrivateKeyInfo);
                }
                else
                {
                    privateKeyInfo = PrivateKeyInfo.GetInstance(_data);
                }

                var algorithmOid = privateKeyInfo.PrivateKeyAlgorithm.Algorithm;
                var key = privateKeyInfo.PrivateKey.GetOctets();
                if (algorithmOid.Equals(PkcsObjectIdentifiers.RsaEncryption))
                {
                    return new RsaKey(key);
                }

                if (algorithmOid.Equals(X9ObjectIdentifiers.IdECPublicKey))
                {
                    var parameters = privateKeyInfo.PrivateKeyAlgorithm.Parameters.GetDerEncoded();
                    var parametersReader = new AsnReader(parameters, AsnEncodingRules.DER);
                    var curve = parametersReader.ReadObjectIdentifier();
                    parametersReader.ThrowIfNotEmpty();

                    var privateKeyReader = new AsnReader(key, AsnEncodingRules.DER);
                    var sequenceReader = privateKeyReader.ReadSequence();
                    privateKeyReader.ThrowIfNotEmpty();

                    var version = sequenceReader.ReadInteger();
                    if (version != BigInteger.One)
                    {
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "EC version '{0}' is not supported.", version));
                    }

                    var privatekey = sequenceReader.ReadOctetString();

                    var publicKeyReader = sequenceReader.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true));
                    var publickey = publicKeyReader.ReadBitString(out _);
                    publicKeyReader.ThrowIfNotEmpty();

                    sequenceReader.ThrowIfNotEmpty();

                    return new EcdsaKey(curve, publickey, privatekey.TrimLeadingZeros());
                }

                if (algorithmOid.Equals(EdECObjectIdentifiers.id_Ed25519))
                {
                    // RFC 8410 section 7: the privateKey field holds a CurvePrivateKey OCTET STRING.
                    var curvePrivateKeyReader = new AsnReader(key, AsnEncodingRules.DER);
                    var seed = curvePrivateKeyReader.ReadOctetString();
                    curvePrivateKeyReader.ThrowIfNotEmpty();

                    if (seed.Length != Ed25519.SecretKeySize)
                    {
                        throw new SshException(string.Format(CultureInfo.InvariantCulture, "Invalid Ed25519 private key length {0}.", seed.Length));
                    }

                    return new ED25519Key(seed);
                }

                throw new SshException(string.Format(CultureInfo.InvariantCulture, "Private key algorithm \"{0}\" is not supported.", algorithmOid));
            }
        }
    }
}
