using System;
using System.Runtime.Serialization;

namespace Renci.SshClient.Common
{
    [Serializable]
    public class SshException : Exception
    {
        public SshException()
        {
        }

        public SshException(string message)
            : base(message)
        {
        }

        public SshException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SshException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
