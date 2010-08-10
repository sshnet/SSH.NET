
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
            //  TODO:   Keep track of all open channels to cdisconnect them when connection is closed
            var channel = new ChannelSession(this._session.SessionInfo);

            var result = channel.Execute(command);

            return result;
        }
    }
}
