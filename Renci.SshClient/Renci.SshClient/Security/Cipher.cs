using System.Collections.Generic;
using System.Linq;
using System;

namespace Renci.SshClient.Security
{
    public abstract class Cipher : Algorithm, IDisposable
    {
        public abstract int BlockSize { get; }

        public abstract int KeySize { get; }

        protected byte[] Key { get; private set; }

        protected byte[] Vector { get; private set; }

        public virtual void Init(IEnumerable<byte> key, IEnumerable<byte> vector)
        {
            this.Key = key.ToArray();
            this.Vector = vector.ToArray();
        }

        public abstract IEnumerable<byte> Encrypt(IEnumerable<byte> data);

        public abstract IEnumerable<byte> Decrypt(IEnumerable<byte> data);
        
        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~Cipher()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}
