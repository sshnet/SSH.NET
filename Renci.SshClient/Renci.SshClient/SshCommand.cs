using System;
using System.IO;
using System.Text;
using Renci.SshClient.Channels;

namespace Renci.SshClient
{
    public class SshCommand : IDisposable
    {
        private Encoding _encoding;

        private Session _session;

        private ChannelSessionExec _channel;

        public string CommandText { get; private set; }

        //  TODO:   Ensure this property is utilize and timeout can be change per command
        public int CommandTimeout { get; private set; }

        public uint ExitStatus
        {
            get
            {
                return this._channel.ExitStatus;
            }
        }

        public MemoryStream OutputStream { get; private set; }

        public MemoryStream ExtendedOutputStream { get; private set; }

        public string Result
        {
            get
            {
                return this._encoding.GetString(this.OutputStream.ToArray());
            }
        }

        public string Error
        {
            get
            {
                if (this._channel.HasError)
                    return this._encoding.GetString(this.ExtendedOutputStream.ToArray());
                else
                    return string.Empty;
            }
        }

        internal SshCommand(Session session, string commandText)
            : this(session, commandText, Encoding.ASCII)
        {
        }

        internal SshCommand(Session session, string commandText, Encoding encoding)
        {
            this._encoding = encoding;
            this._session = session;
            this.CommandText = commandText;

            this._channel = this._session.CreateChannel<ChannelSessionExec>();
            this.OutputStream = new MemoryStream();
            this.ExtendedOutputStream = new MemoryStream();
        }

        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            //  When comman rexecuted again, create a new channel
            if (this._channel != null)
            {
                this._channel = this._session.CreateChannel<ChannelSessionExec>();
                this.OutputStream = new MemoryStream();
                this.ExtendedOutputStream = new MemoryStream();
            }

            if (string.IsNullOrEmpty(this.CommandText))
                throw new ArgumentException("CommandText property is empty.");

            return this._channel.BeginExecute(this.CommandText, this.OutputStream, this.ExtendedOutputStream, callback, state);
        }

        public IAsyncResult BeginExecute(string commandText, AsyncCallback callback, object state)
        {
            this.CommandText = commandText;
            return BeginExecute(callback, state);
        }

        public string EndExecute(IAsyncResult asynchResult)
        {
            ChannelAsyncResult channelAsyncResult = asynchResult as ChannelAsyncResult;

            channelAsyncResult.Channel.EndExecute(asynchResult);

            return this.Result;
        }

        public string Execute()
        {
            return this.EndExecute(this.BeginExecute(null, null));
        }

        public string Execute(string commandText)
        {
            this.CommandText = commandText;
            return this.Execute();
        }


        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.OutputStream != null)
                    {
                        this.OutputStream.Dispose();
                    }

                    // Dispose managed resources.
                    if (this.ExtendedOutputStream != null)
                    {
                        this.ExtendedOutputStream.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        ~SshCommand()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
