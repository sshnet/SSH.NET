using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Utilities
{
    internal abstract class Integers
    {
        public static int RotateLeft(int i, int distance)
        {
            return (i << distance) ^ (int)((uint)i >> -distance);
        }

        public static uint RotateLeft(uint i, int distance)
        {
            return (i << distance) ^ (i >> -distance);
        }

        public static int RotateRight(int i, int distance)
        {
            return (int)((uint)i >> distance) ^ (i << -distance);
        }

        public static uint RotateRight(uint i, int distance)
        {
            return (i >> distance) ^ (i << -distance);
        }
    }
}
