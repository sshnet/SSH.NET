namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal sealed class MatchFormatter : IMatchFormatter
    {
        public string Format(Match match)
        {
            using (var writer = new StringWriter())
            {
                Format(match, writer);
                return writer.ToString();
            }
        }

        public void Format(Match match, TextWriter writer)
        {
            writer.Write("Match ");

            if (match.Users.Length > 0)
            {
                writer.Write("User ");
                for (var i = 0; i < match.Users.Length; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(',');
                    }

                    writer.Write(match.Users[i]);
                }
            }

            if (match.Addresses.Length > 0)
            {
                writer.Write("Address ");
                for (var i = 0; i < match.Addresses.Length; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(',');
                    }

                    writer.Write(match.Addresses[i]);
                }
            }

            writer.WriteLine();

            if (match.AuthenticationMethods != null)
            {
                writer.WriteLine("    AuthenticationMethods " + match.AuthenticationMethods);
            }
        }
    }
}
