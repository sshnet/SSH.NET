using System.Collections.Generic;
using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Tests.Common
{
    internal static class Extensions
    {
        public static string AsString(this IList<ExceptionEventArgs> exceptionEvents)
        {
            if (exceptionEvents.Count == 0)
                return string.Empty;

            string reportedExceptions = string.Empty;
            foreach (var exceptionEvent in exceptionEvents)
                reportedExceptions += exceptionEvent.Exception.ToString();

            return reportedExceptions;
        }

        public static byte[] Copy(this byte[] buffer)
        {
            var copy = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, copy, 0, buffer.Length);
            return copy;
        }
    }
}
