using System;
using System.Collections.Generic;

namespace Renci.SshClient.Sftp
{
    public class SftpFile
    {
        public SftpFile()
        {
            this.Attributes = new SftpFileAttributes();
            this.CreatedTime = DateTime.MinValue;
            this.AccessedTime = DateTime.MinValue;
            this.ModifiedTime = DateTime.MinValue;
            this.Size = -1;
            this.UserId = -1;
            this.GroupId = -1;
            this.Permissions = -1;
            this.Extentions = new Dictionary<string, string>();
        }

        public string Name { get; set; }

        public string AbsolutePath { get; set; }

        public SftpFileAttributes Attributes { get; set; }

        public string Filename { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime AccessedTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        public long Size { get; set; }

        public int UserId { get; set; }

        public int GroupId { get; set; }

        public int Permissions { get; set; }

        public IDictionary<string, string> Extentions { get; set; }
    }
}
