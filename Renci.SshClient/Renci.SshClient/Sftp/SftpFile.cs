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
        /// <param name="absolutePath">Name of the file.</param>
        /// <param name="attributes">The attributes.</param>
        public SftpFile(string absolutePath, Attributes attributes)
        {
            this.AbsolutePath = absolutePath;

            this.Attributes = attributes;
        }

        /// <summary>
        /// Gets file status information attributes.
        /// </summary>
        public Attributes Attributes { get; private set; }

        /// <summary>
        /// Gets or sets file name.
        /// </summary>
        /// <value>
        /// File name.
        /// </value>
        public string AbsolutePath { get; set; }

        /// <summary>
        /// Gets or sets file accessed time.
        /// </summary>
        /// <value>
        /// File accessed time.
        /// </value>
        public DateTime AccessedTime
        {
            get
            {
                if (this.Attributes.AccessTime.HasValue)
                    return this.Attributes.AccessTime.Value;
                else
                    return DateTime.MinValue;
            }
            set
            {
                this.Attributes.AccessTime = value;
            }
        }

        /// <summary>
        /// Gets or sets file modified time.
        /// </summary>
        /// <value>
        /// File modified time.
        /// </value>
        public DateTime ModifiedTime
        {
            get
            {
                if (this.Attributes.ModifyTime.HasValue)
                    return this.Attributes.ModifyTime.Value;
                else
                    return DateTime.MinValue;
            }
            set
            {
                this.Attributes.ModifyTime = value;
            }
        }

        /// <summary>
        /// Gets or sets file size.
        /// </summary>
        /// <value>
        /// File size.
        /// </value>
        public long Size
        {
            get
            {
                if (this.Attributes.Size.HasValue)
                    return (long)this.Attributes.Size.Value;
                else
                    return -1;
            }
            set
            {
                if (value > -1)
                {
                    this.Attributes.Size = new Nullable<ulong>((ulong)value);
                }
                else
                {
                    this.Attributes.Size = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets file user id.
        /// </summary>
        /// <value>
        /// File user id.
        /// </value>
        public int UserId
        {
            get
            {
                if (this.Attributes.UserId.HasValue)
                    return (int)this.Attributes.UserId.Value;
                else
                    return -1;
            }
            set
            {
                if (value > -1)
                {
                    this.Attributes.UserId = new Nullable<uint>((uint)value);
                }
                else
                {
                    this.Attributes.UserId = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets file group id.
        /// </summary>
        /// <value>
        /// File group id.
        /// </value>
        public int GroupId
        {
            get
            {
                if (this.Attributes.GroupId.HasValue)
                    return (int)this.Attributes.GroupId.Value;
                else
                    return -1;
            }
            set
            {
                if (value > -1)
                {
                    this.Attributes.GroupId = new Nullable<uint>((uint)value);
                }
                else
                {
                    this.Attributes.GroupId = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets file permissions.
        /// </summary>
        /// <value>
        /// File permissions.
        /// </value>
        public int Permissions
        {
            get
            {
                if (this.Attributes.Permissions.HasValue)
                    return (int)this.Attributes.Permissions.Value;
                else
                    return -1;
            }
            set
            {
                if (value > -1)
                {
                    this.Attributes.Permissions = new Nullable<uint>((uint)value);
                }
                else
                {
                    this.Attributes.Permissions = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets file extensions attributes.
        /// </summary>
        /// <value>
        /// File extensions.
        /// </value>
        public IDictionary<string, string> Extensions
        {
            get
            {
                return this.Attributes.Extensions;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Name {0}, Size {1}, User ID {2}, Group ID {3}, Permissions {4:X}, Accessed {5}, Modified {6}", this.AbsolutePath, this.Size, this.UserId, this.GroupId, this.Permissions, this.AccessedTime, this.ModifiedTime);
        }
    }
}
