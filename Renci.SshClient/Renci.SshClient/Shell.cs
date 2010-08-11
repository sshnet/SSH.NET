
using System.IO;
using System.Text;
using Renci.SshClient.Channels;
namespace Renci.SshClient
{
    public class Shell
    {
        private Session _session;

        internal Shell(Session session)
        {
            this._session = session;
        }

        public string Execute(string command)
        {
            //var channel = new ChannelSession(this._session);

            MemoryStream resultStream = new MemoryStream();


            var channel = this._session.CreateChannel<ChannelExec>();

            channel.Execute(command, resultStream, null);

            return Encoding.ASCII.GetString(resultStream.ToArray());
        }

        public string Execute(string command, Stream extended)
        {
            MemoryStream resultStream = new MemoryStream();

            var channel = this._session.CreateChannel<ChannelExec>();

            channel.Execute(command, resultStream, extended);

            return Encoding.ASCII.GetString(resultStream.ToArray());
        }

    }
}
