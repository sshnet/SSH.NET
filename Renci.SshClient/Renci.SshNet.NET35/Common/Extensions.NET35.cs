using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Common
{
    public static class ExtensionsNET35
    {
        internal static bool IsNullOrWhiteSpace(this string s)
        {
            if (s == null) 
                return true;

            for (var i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s, i))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
