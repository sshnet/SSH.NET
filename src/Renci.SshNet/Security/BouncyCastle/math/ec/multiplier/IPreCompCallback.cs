using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.EC.Multiplier
{
    internal interface IPreCompCallback
    {
        PreCompInfo Precompute(PreCompInfo existing);
    }
}
