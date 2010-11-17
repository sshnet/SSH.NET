
using System.IO;
using Renci.SshClient.Channels;
namespace Renci.SshClient
{
    public class Shell
    {
        private readonly Session _session;

        private ChannelSessionShell _channel;

        internal Shell(Session session)
        {
            this._session = session;
        }

        public void Connect(Stream output, Stream extendedOutput)
        {
            this._channel = this._session.CreateChannel<ChannelSessionShell>();
            this._channel.Start(output, extendedOutput);
        }

        public void Send(string data)
        {
            this._channel.Send(data);
        }

        public void Disconnect()
        {
            this._channel.Close();
        }
    }
}
