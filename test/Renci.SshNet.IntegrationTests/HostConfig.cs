using System.Net;
using System.Text.RegularExpressions;

namespace Renci.SshNet.IntegrationTests
{
    class HostConfig
    {
        private static readonly Regex HostsEntryRegEx = new Regex(@"^(?<IPAddress>[\S]+)\s+(?<HostName>[a-zA-Z]+[a-zA-Z\-\.]*[a-zA-Z]+)\s*(?<Aliases>.+)*$", RegexOptions.Singleline);

        public List<HostEntry> Entries { get; }

        private HostConfig()
        {
            Entries = new List<HostEntry>();
        }

        public static HostConfig Read(ScpClient scpClient, string path)
        {
            HostConfig hostConfig = new HostConfig();

            using (var ms = new MemoryStream())
            {
                scpClient.Download(path, ms);
                ms.Position = 0;

                using (var sr = new StreamReader(ms, Encoding.ASCII))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // skip comments
                        if (line.StartsWith("#"))
                        {
                            continue;
                        }

                        var hostEntryMatch = HostsEntryRegEx.Match(line);
                        if (!hostEntryMatch.Success)
                        {
                            continue;
                        }

                        var entryIPAddress = hostEntryMatch.Groups["IPAddress"].Value;
                        var entryAliasesGroup = hostEntryMatch.Groups["Aliases"];

                        var entry = new HostEntry(IPAddress.Parse(entryIPAddress), hostEntryMatch.Groups["HostName"].Value);

                        if (entryAliasesGroup.Success)
                        {
                            var aliases = entryAliasesGroup.Value.Split(' ');
                            foreach (var alias in aliases)
                            {
                                entry.Aliases.Add(alias);
                            }
                        }

                        hostConfig.Entries.Add(entry);
                    }
                }
            }

            return hostConfig;
        }

        public void Write(ScpClient scpClient, string path)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms, Encoding.ASCII))
            {
                // Use linux line ending
                sw.NewLine = "\n";
                     
                foreach (var hostEntry in Entries)
                {
                    sw.Write(hostEntry.IPAddress);
                    sw.Write("    ");
                    sw.Write(hostEntry.HostName);

                    if (hostEntry.Aliases.Count > 0)
                    {
                        sw.Write("    ");
                        for (var i = 0; i < hostEntry.Aliases.Count; i++)
                        {
                            if (i > 0)
                            {
                                sw.Write(' ');
                            }
                            sw.Write(hostEntry.Aliases[i]);
                        }
                    }
                    sw.WriteLine();
                }

                sw.Flush();
                ms.Position = 0;

                scpClient.Upload(ms, path);
            }
        }
    }

    public class HostEntry
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
