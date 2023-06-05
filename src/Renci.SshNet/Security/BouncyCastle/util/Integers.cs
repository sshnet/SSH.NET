using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Utilities
{
    internal abstract class Integers
    {
        public static int RotateLeft(int i, int distance)
        {
            return (i << distance) ^ (int)((uint)i >> -distance);
        }
    }
}
