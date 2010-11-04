
using System;
using System.IO;
using System.Text;
using Renci.SshClient.Channels;
namespace Renci.SshClient
{
    public class Shell
    {
        private readonly Session _session;

        internal Shell(Session session)
        {
            this._session = session;
        }

        public string Execute(string command)
        {
            return this.Execute(command, null);
        }

        public string Execute(string command, Stream extended)
        {
            using (MemoryStream resultStream = new MemoryStream())
            {
                this.Execute(command, resultStream, extended);

                return Encoding.ASCII.GetString(resultStream.ToArray());
            }
        }

        public void Execute(string command, Stream output, Stream extended)
        {
            this.EndExecute(this.BeginExecute(command, output, extended, null, null));
        }

        public IAsyncResult BeginExecute(string command, Stream output, AsyncCallback callback, object state)
        {
            return this.BeginExecute(command, output, null, callback, state);
        }

        public IAsyncResult BeginExecute(string command, Stream output, Stream extendedOutput, AsyncCallback callback, object state)
        {
            var channel = this._session.CreateChannel<ChannelSessionExec>();

            return channel.BeginExecute(command, output, extendedOutput, callback, state);
        }

        public void EndExecute(IAsyncResult asynchResult)
        {
            ChannelAsyncResult channelAsyncResult = asynchResult as ChannelAsyncResult;

            channelAsyncResult.Channel.EndExecute(asynchResult);
        }
    }
}
