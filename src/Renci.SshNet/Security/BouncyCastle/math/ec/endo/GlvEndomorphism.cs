using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.EC.Endo
{
    internal interface GlvEndomorphism
        :   ECEndomorphism
    {
        BigInteger[] DecomposeScalar(BigInteger k);
    }
}
