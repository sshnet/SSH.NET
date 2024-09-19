#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Implements certificate support for host algorithm.
    /// </summary>
    public class CertificateHostAlgorithm : KeyHostAlgorithm
    {
        /// <summary>
        /// The <see cref="KeyHostAlgorithm"/> factories which may be used in order to verify
        /// the signature within the certificate.
        /// </summary>
        private readonly IReadOnlyDictionary<string, Func<byte[], KeyHostAlgorithm>>? _keyAlgorithms;

        /// <summary>
        /// Gets certificate used in this host key algorithm.
        /// </summary>
        public Certificate Certificate { get; }

        /// <inheritdoc/>
        internal override SshKeyData KeyData
        {
            get
            {
                return Certificate.KeyData;
            }
        }

        /// <summary>
        /// Gets the encoded bytes of the certificate.
        /// </summary>
        public override byte[] Data
        {
            get { return Certificate.Bytes; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The algorithm identifier.</param>
        /// <param name="privateKey">The private key used for this host algorithm.</param>
        /// <param name="certificate">The certificate which certifies <paramref name="privateKey"/>.</param>
        public CertificateHostAlgorithm(string name, Key privateKey, Certificate certificate)
            : base(name, privateKey)
        {
            Certificate = certificate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The algorithm identifier.</param>
        /// <param name="privateKey">The private key used for this host algorithm.</param>
        /// <param name="certificate">The certificate which certifies <paramref name="privateKey"/>.</param>
        /// <param name="digitalSignature"><inheritdoc cref="KeyHostAlgorithm.DigitalSignature" path="/summary"/></param>
        public CertificateHostAlgorithm(string name, Key privateKey, Certificate certificate, DigitalSignature digitalSignature)
            : base(name, privateKey, digitalSignature)
        {
            Certificate = certificate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The algorithm identifier.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="keyAlgorithms"><inheritdoc cref="_keyAlgorithms" path="/summary"/></param>
        public CertificateHostAlgorithm(string name, Certificate certificate, IReadOnlyDictionary<string, Func<byte[], KeyHostAlgorithm>> keyAlgorithms)
            : base(name, certificate.Key)
        {
            Certificate = certificate;
            _keyAlgorithms = keyAlgorithms;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHostAlgorithm"/> class.
        /// </summary>
        /// <param name="name">The algorithm identifier.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="digitalSignature"><inheritdoc cref="KeyHostAlgorithm.DigitalSignature" path="/summary"/></param>
        /// <param name="keyAlgorithms"><inheritdoc cref="_keyAlgorithms" path="/summary"/></param>
        public CertificateHostAlgorithm(string name, Certificate certificate, DigitalSignature digitalSignature, IReadOnlyDictionary<string, Func<byte[], KeyHostAlgorithm>> keyAlgorithms)
            : base(name, certificate.Key, digitalSignature)
        {
            Certificate = certificate;
            _keyAlgorithms = keyAlgorithms;
        }

        /// <inheritdoc/>
        public override byte[] Sign(byte[] data)
        {
            Debug.Assert("-cert-v01@openssh.com".Length == 21);

            var signatureFormatIdentifier = Name.EndsWith("-cert-v01@openssh.com", StringComparison.Ordinal)
                ? Name.Substring(0, Name.Length - 21)
                : Name;

            return new SignatureKeyData(signatureFormatIdentifier, DigitalSignature.Sign(data)).GetBytes();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data to verify the signature against.</param>
        /// <param name="signatureBlob">The signature blob in format specific encoding.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="signatureBlob"/> is the result of signing
        /// <paramref name="data"/> with the corresponding private key to <see cref="Certificate"/>,
        /// and <see cref="Certificate"/> is valid with respect to its validity period and to its
        /// signature therein as signed by the certificate authority.
        /// </returns>
        internal override bool VerifySignatureBlob(byte[] data, byte[] signatureBlob)
        {
            // Validate the session signature against the public key as normal.

            if (!base.VerifySignatureBlob(data, signatureBlob))
            {
                return false;
            }

            // Validate the validity period of the certificate.

            var unixNow = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (unixNow < Certificate.ValidAfterUnixSeconds || unixNow > Certificate.ValidBeforeUnixSeconds)
            {
                return false;
            }

            // Validate the certificate (i.e. the signature contained within) against
            // the CA public key (also contained in the certificate).

            var certSignatureData = new SignatureKeyData();
            certSignatureData.Load(Certificate.Signature);

            if (_keyAlgorithms is null)
            {
                throw new InvalidOperationException($"Invalid usage of {nameof(CertificateHostAlgorithm)}.{nameof(VerifySignature)}. " +
                    $"Use a constructor which passes key algorithms.");
            }

            return _keyAlgorithms.TryGetValue(certSignatureData.AlgorithmName, out var keyAlgFactory) &&
                keyAlgFactory(Certificate.CertificateAuthorityKey).VerifySignatureBlob(Certificate.BytesForSignature, certSignatureData.Signature);
        }
    }
}
