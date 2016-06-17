using System;

namespace Renci.SshNet.Messages
{

    /// <summary>
    /// Indicates that a class represents SSH message. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class MessageAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets message name as defined in RFC 4250.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets message number as defined in RFC 4250.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        public byte Number { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="number">The number.</param>
        public MessageAttribute(string name, byte number)
        {
            Name = name;
            Number = number;
        }
    }
}
