﻿using System.Globalization;

namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal sealed class Int32Formatter
    {
        public string Format(int value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}
