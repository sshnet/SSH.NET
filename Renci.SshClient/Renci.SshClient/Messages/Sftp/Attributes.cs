using System;
using System.Collections.Generic;

namespace Renci.SshClient.Messages.Sftp
{
    public class Attributes
    {
        public ulong? Size { get; set; }

        public uint? UserId { get; set; }

        public uint? GroupId { get; set; }

        public uint? Permissions { get; set; }

        public DateTime? AccessTime { get; set; }

        public DateTime? ModifyTime { get; set; }

        public IDictionary<string, string> Extentions { get; set; }
    }
}
