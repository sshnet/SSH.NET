using System;

namespace Renci.SshClient
{
    internal class RequestSuccessEventArgs : EventArgs
    {
        public uint BoundPort { get; set; }
    }
}
