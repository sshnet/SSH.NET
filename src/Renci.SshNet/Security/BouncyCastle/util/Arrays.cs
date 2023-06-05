using System;
using System.Text;

using Renci.SshNet.Security.Org.BouncyCastle.Math;

namespace Renci.SshNet.Security.Org.BouncyCastle.Utilities
{
    /// <summary> General array utilities.</summary>
    internal abstract class Arrays
    {
        public static readonly byte[] EmptyBytes = new byte[0];
        public static readonly int[] EmptyInts = new int[0];

        public static bool AreEqual(
            int[]	a,
            int[]	b)
        {
            if (a == b)
                return true;

            if (a == null || b == null)
                return false;

            return HaveSameContents(a, b);
        }

        private static bool HaveSameContents(
            int[]	a,
            int[]	b)
        {
            int i = a.Length;
            if (i != b.Length)
                return false;
            while (i != 0)
            {
                --i;
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        public static int GetHashCode(int[] data)
        {
            if (data == null)
                return 0;

            int i = data.Length;
            int hc = i + 1;

            while (--i >= 0)
            {
                hc *= 257;
                hc ^= data[i];
            }

            return hc;
        }

        public static byte[] Clone(
            byte[] data)
        {
            return data == null ? null : (byte[])data.Clone();
        }

        public static int[] Clone(
            int[] data)
        {
            return data == null ? null : (int[])data.Clone();
        }

        public static long[] Clone(long[] data)
        {
            return data == null ? null : (long[])data.Clone();
        }

        public static void Fill(
            byte[]	buf,
            byte	b)
        {
            int i = buf.Length;
            while (i > 0)
            {
                buf[--i] = b;
            }
        }
    }
}
