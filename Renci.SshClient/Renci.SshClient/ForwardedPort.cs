
using System;
namespace Renci.SshClient
{
    public abstract class ForwardedPort
    {
        internal Session Session { get; set; }

        public uint BoundPort { get; internal set; }

        public string ConnectedHost { get; internal set; }

        public uint ConnectedPort { get; internal set; }

        public event EventHandler<ExceptionEventArgs> Exception;

        internal ForwardedPort()
        {

        }

        public virtual void Start()
        {
            if (this.Session == null)
            {
                throw new InvalidOperationException("Session property is null.");
            }

            if (!this.Session.IsConnected)
            {
                throw new InvalidOperationException("Not connected.");
            }
        }

        public abstract void Stop();

        protected void RaiseExceptionEvent(Exception execption)
        {
            if (this.Exception != null)
            {
                this.Exception(this, new ExceptionEventArgs(execption));
            }
        }

    }
}
