
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

            var channel = this._session.CreateChannel<ChannelSession>();

            var result = channel.Execute(command);

            return result;
        }
    }
}
