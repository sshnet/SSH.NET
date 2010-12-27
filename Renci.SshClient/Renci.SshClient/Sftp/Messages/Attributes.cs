using System;
using System.Collections.Generic;

namespace Renci.SshClient.Sftp.Messages
{
    /// <summary>
    /// Represents SFTP File attribute.
    /// </summary>
    public class Attributes
    {
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public ulong? Size { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>
        /// The user id.
        /// </value>
        public uint? UserId { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        public uint? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the permissions.
        /// </summary>
        /// <value>
        /// The permissions.
        /// </value>
        public uint? Permissions { get; set; }

        /// <summary>
        /// Gets or sets the access time.
        /// </summary>
        /// <value>
        /// The access time.
        /// </value>
        public DateTime? AccessTime { get; set; }

        /// <summary>
        /// Gets or sets the modify time.
        /// </summary>
        /// <value>
        /// The modify time.
        /// </value>
        public DateTime? ModifyTime { get; set; }

        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        public IDictionary<string, string> Extensions { get; set; }
    }
}
