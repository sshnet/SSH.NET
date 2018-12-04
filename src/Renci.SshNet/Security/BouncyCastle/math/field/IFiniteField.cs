using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.Field
{
    internal interface IFiniteField
    {
        BigInteger Characteristic { get; }

        int Dimension { get; }
    }
}
