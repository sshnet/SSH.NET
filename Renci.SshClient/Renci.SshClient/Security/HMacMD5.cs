using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Security
{
    internal class HMacMD5 : HMac
    {
        public override string Name
        {
            get { return "hmac-md5"; }
        }

        public override void Init(IEnumerable<byte> key)
        {
            this._hmac = new System.Security.Cryptography.HMACMD5(key.Take(16).ToArray());
        }
    }
}
