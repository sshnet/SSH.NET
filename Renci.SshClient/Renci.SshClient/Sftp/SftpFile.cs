using System;
using System.Collections.Generic;
using Renci.SshClient.Messages.Sftp;

namespace Renci.SshClient.Sftp
{
    public class SftpFile
    {
        public SftpFile(string fileName, string fullName, Attributes attributes)
        {
            this.Name = fileName;

            //this.AbsolutePath = fullName;

            if (attributes.Size.HasValue)
                this.Size = (int)attributes.Size.Value;
            else
                this.Size = -1;

            if (attributes.UserId.HasValue)
                this.UserId = (int)attributes.UserId.Value;
            else
                this.UserId = -1;

            if (attributes.GroupId.HasValue)
                this.GroupId = (int)attributes.GroupId.Value;
            else
                this.GroupId = -1;

            if (attributes.Permissions.HasValue)
                this.Permissions = (int)attributes.Permissions.Value;
            else
                this.Permissions = -1;

            if (attributes.AccessTime.HasValue)
                this.AccessedTime = attributes.AccessTime.Value;
            else
                this.AccessedTime = DateTime.MinValue;

            if (attributes.ModifyTime.HasValue)
                this.ModifiedTime = attributes.ModifyTime.Value;
            else
                this.ModifiedTime = DateTime.MinValue;

            //this.CreatedTime = DateTime.MinValue;

            if (attributes.Extentions != null)
                this.Extentions = new Dictionary<string, string>(attributes.Extentions);
        }

        public string Name { get; set; }

        //public string AbsolutePath { get; set; }

        public string Filename { get; set; }

        //public DateTime CreatedTime { get; set; }

        public DateTime AccessedTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        public long Size { get; set; }

        public int UserId { get; set; }

        public int GroupId { get; set; }

        public int Permissions { get; set; }

        public IDictionary<string, string> Extentions { get; set; }

        public override string ToString()
        {
            return string.Format("Name {0}, Size {1}, User ID {2}, Group ID {3}, Permissions {4:X}, Accessed {5}, Modified {6}", this.Name, this.Size, this.UserId, this.GroupId, this.Permissions, this.AccessedTime, this.ModifiedTime);
        }
    }
}
