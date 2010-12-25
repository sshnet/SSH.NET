using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Messages
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class MessageAttribute : Attribute
    {
        public string Name { get; set; }

        public byte Number { get; set; }

        // This is a positional argument
        public MessageAttribute(string name, byte number)
        {
            this.Name = name;
            this.Number = number;
        }
    }
}
