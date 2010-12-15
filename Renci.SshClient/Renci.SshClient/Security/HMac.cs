using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Security
{
    public abstract class HMac : Algorithm, IDisposable
    {
        protected System.Security.Cryptography.HMAC _hmac;

        public abstract void Init(IEnumerable<byte> key);

        internal byte[] ComputeHash(byte[] hashData)
        {
            return this._hmac.ComputeHash(hashData);
        }

        public int HashSize
        {
            get
            {
                return this._hmac.HashSize;
            }
        }

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._hmac != null)
                    {
                        this._hmac.Dispose();
                    }
                }

                // Note disposing has been done.
                this._disposed = true;
            }
        }

        ~HMac()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
