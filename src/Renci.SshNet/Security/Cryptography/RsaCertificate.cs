using System;
using System.Collections.Generic;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class RsaCertificate : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public BigInteger Nonce { get; private set; }

        /// <summary>
        /// Gets the exponent.
        /// </summary>
        public BigInteger Exponent { get; private set; }

        /// <summary>
        /// Gets the modulus.
        /// </summary>
        public BigInteger Modulus { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public  ulong Serial { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string KeyId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<string> ValidPrinciples { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ValidBefore { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ValidAfter { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<string> CriticalOptions { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<string> Extensions { get; private set; }


        private byte[] Reserved { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BigInteger SignatureKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public BigInteger Signature { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RsaCertificate"/> class.
        /// </summary>
        public RsaCertificate()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaCertificate"/> class.
        /// </summary>
        public RsaCertificate(string name, BigInteger nonce, BigInteger exponent, BigInteger modulus, ulong serial, uint type, string keyId, 
                                IList<string> validPrinciples, DateTime validAfter, DateTime validBefore, IList<string> extensions,
                                IList<string> criticalOptions, BigInteger signatureKey, BigInteger signature)
        {
            Name = name;
            Nonce = nonce;
            Serial = serial;
            Exponent = exponent;
            Modulus = modulus;
            Type = type;
            KeyId = keyId;
            ValidPrinciples = validPrinciples;
            ValidAfter = validAfter;
            ValidBefore = validBefore;
            CriticalOptions = criticalOptions;
            Extensions = extensions;
            SignatureKey = signatureKey;
            Signature = signature;
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
        /// <see cref="RsaCertificate"/> is reclaimed by garbage collection.
        /// </summary>
        ~RsaCertificate()
        {
            Dispose(false);
        }

        #endregion
    }
}
