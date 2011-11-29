using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Common
{
    public class HostKeyEventArgs : EventArgs
    {
        public bool CanTrust { get; set; }

        public byte[] HostKey { get; private set; }

        public byte[] FingerPrint { get; private set; }

        public HostKeyEventArgs(byte[] hostKey)
        {
            this.CanTrust = true;   //  Set default value

            this.HostKey = hostKey;

            using (var md5 = new MD5Hash())
            {
                this.FingerPrint = md5.ComputeHash(hostKey);
            }
        }
    }
}
