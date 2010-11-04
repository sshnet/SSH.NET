using System;

namespace Renci.SshClient
{
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public ExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}
