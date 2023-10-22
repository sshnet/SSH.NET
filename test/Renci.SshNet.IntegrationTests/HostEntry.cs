using System.Net;

namespace Renci.SshNet.IntegrationTests
{
    public sealed class HostEntry
    {
        public HostEntry(IPAddress ipAddress, string hostName)
        {
            IPAddress = ipAddress;
            HostName = hostName;
            Aliases = new List<string>();
        }

        public IPAddress IPAddress { get; private set; }

        public string HostName { get; set; }

        public List<string> Aliases { get; }
    }
}
