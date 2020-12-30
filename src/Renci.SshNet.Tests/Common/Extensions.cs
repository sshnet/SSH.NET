using System.Collections.Generic;
using Renci.SshNet.Common;
using System;
using Renci.SshNet.Sftp;

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

        /// <summary>
        /// Creates a deep clone of the current instance.
        /// </summary>
        /// <returns>
        /// A deep clone of the current instance.
        /// </returns>
        internal static SftpFileAttributes Clone(this SftpFileAttributes value)
        {
            Dictionary<string, string> clonedExtensions;

            if (value.Extensions != null)
            {
                clonedExtensions = new Dictionary<string, string>(value.Extensions.Count);
                foreach (var entry in value.Extensions)
                {
                    clonedExtensions.Add(entry.Key, entry.Value);
                }
            }
            else
            {
                clonedExtensions = null;
            }

            return new SftpFileAttributes(value.LastAccessTimeUtc,
                                          value.LastWriteTimeUtc,
                                          value.Size,
                                          value.UserId,
                                          value.GroupId,
                                          value.Permissions,
                                          clonedExtensions);
        }
    }
}
