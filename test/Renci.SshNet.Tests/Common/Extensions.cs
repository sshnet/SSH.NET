#nullable enable
using System;
using System.Collections.Generic;

using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;

namespace Renci.SshNet.Tests.Common
{
    internal static class Extensions
    {
        public static string AsString(this IList<ExceptionEventArgs> exceptionEvents)
        {
            if (exceptionEvents.Count == 0)
            {
                return string.Empty;
            }

            var reportedExceptions = string.Empty;
            foreach (var exceptionEvent in exceptionEvents)
            {
                reportedExceptions += exceptionEvent.Exception.ToString();
            }

            return reportedExceptions;
        }

        /// <returns>[4 bytes] || packet_len || padding_len || payload || padding.</returns>
        public static byte[] GetPacket(this Message message, byte paddingMultiplier, Compressor? compressor)
        {
            var buffer = Array.Empty<byte>();

            var byteCount = message.GetPacket(
                ref buffer,
                paddingMultiplier,
                compressor,
                excludePacketLengthFieldWhenPadding: false,
                macLength: 0);

            Array.Resize(ref buffer, byteCount);

            return buffer;
        }
    }
}
