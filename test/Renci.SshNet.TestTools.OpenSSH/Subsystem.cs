using System.Text.RegularExpressions;

namespace Renci.SshNet.TestTools.OpenSSH
{
    public sealed class Subsystem
    {
        public Subsystem(string name, string command)
        {
            Name = name;
            Command = command;
        }

        public string Name { get; }

        public string Command { get; set; }

        public static Subsystem FromConfig(string value)
        {
            var subSystemValueRegex = new Regex(@"^\s*(?<name>[\S]+)\s+(?<command>.+?){1}\s*$");

            var match = subSystemValueRegex.Match(value);
            if (match.Success)
            {
                var nameGroup = match.Groups["name"];
                var commandGroup = match.Groups["command"];

                var name = nameGroup.Value;
                var command = commandGroup.Value;

                return new Subsystem(name, command);
            }

            throw new ArgumentException($"'{value}' not recognized as value for Subsystem.",
                                        nameof(value));
        }

        public void WriteTo(TextWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteLine(Name + "=" + Command);
        }
    }
}
