using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.EC
{
    internal interface ECPointMap
    {
        ECPoint Map(ECPoint p);
    }
}
