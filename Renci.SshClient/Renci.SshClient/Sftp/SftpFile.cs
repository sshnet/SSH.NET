using System;
using System.Collections.Generic;
using Renci.SshClient.Sftp.Messages;

namespace Renci.SshClient.Sftp
{
    /// <summary>
    /// Represents SFTP file information
    /// </summary>
    public class SftpFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SftpFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="attributes">The attributes.</param>
        public SftpFile(string fileName, string fullName, Attributes attributes)
        {
            this.Name = fileName;

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

            if (attributes.Extensions != null)
                this.Extensions = new Dictionary<string, string>(attributes.Extensions);
        }

        /// <summary>
        /// Gets or sets file name.
        /// </summary>
        /// <value>
        /// File name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets file filename.
        /// </summary>
        /// <value>
        /// File filename.
        /// </value>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets file accessed time.
        /// </summary>
        /// <value>
        /// File accessed time.
        /// </value>
        public DateTime AccessedTime { get; set; }

        /// <summary>
        /// Gets or sets file modified time.
        /// </summary>
        /// <value>
        /// File modified time.
        /// </value>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets file size.
        /// </summary>
        /// <value>
        /// File size.
        /// </value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets file user id.
        /// </summary>
        /// <value>
        /// File user id.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets file group id.
        /// </summary>
        /// <value>
        /// File group id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets file permissions.
        /// </summary>
        /// <value>
        /// File permissions.
        /// </value>
        public int Permissions { get; set; }

        /// <summary>
        /// Gets or sets file extensions attributes.
        /// </summary>
        /// <value>
        /// File extensions.
        /// </value>
        public IDictionary<string, string> Extensions { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Name {0}, Size {1}, User ID {2}, Group ID {3}, Permissions {4:X}, Accessed {5}, Modified {6}", this.Name, this.Size, this.UserId, this.GroupId, this.Permissions, this.AccessedTime, this.ModifiedTime);
        }
    }
}
