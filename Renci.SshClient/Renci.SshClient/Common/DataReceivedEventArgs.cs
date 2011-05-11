using System;

namespace Renci.SshClient.Common
{
    internal class DataReceivedEventArgs : EventArgs
    {
        public string Data { get; private set; }

        public DataReceivedEventArgs(string data)
        {
            this.Data = data;
        }
    }
}
