using System;
using System.Collections.Generic;

namespace Renci.SshClient.Common
{
    public class FtpFileInfo
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastModifyTime { get; set; }

        public ulong Size { get; set; }

        public uint UserId { get; set; }

        public uint GroupId { get; set; }

        public uint Permissions { get; set; }

        public IDictionary<string, string> Extentions { get; set; }

    }
}
