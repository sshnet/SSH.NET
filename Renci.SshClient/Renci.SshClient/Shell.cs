
using System;
using System.IO;
using System.Text;
using Renci.SshClient.Channels;
namespace Renci.SshClient
{
    public class Shell : SshBase
    {
        public Shell(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
        }

        public Shell(string host, int port, string username, string password)
            : base(host, port, username, password)
        {
        }

        public Shell(string host, string username, string password)
            : base(host, username, password)
        {
        }

        public Shell(string host, int port, string username, PrivateKeyFile keyFile)
            : base(host, port, username, keyFile)
        {
        }

        public Shell(string host, string username, PrivateKeyFile keyFile)
            : base(host, username, keyFile)
        {
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
            //  Make sure session is connected
            this.Session.Connect();

            var channel = this.Session.CreateChannel<ChannelExec>();

            return channel.BeginExecute(command, output, extendedOutput, callback, state);
        }

        public void EndExecute(IAsyncResult asynchResult)
        {
            ChannelAsyncResult channelAsyncResult = asynchResult as ChannelAsyncResult;

            channelAsyncResult.Channel.EndExecute(asynchResult);
        }
    }
}
