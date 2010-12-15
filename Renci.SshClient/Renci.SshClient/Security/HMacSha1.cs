using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    internal class HMacSha1 : HMac
    {
        public override string Name
        {
            get { return "hmac-sha1"; }
        }

        public override void Init(IEnumerable<byte> key)
        {
            this._hmac = new System.Security.Cryptography.HMACSHA1(key.Take(20).ToArray());
        }
    }
}
