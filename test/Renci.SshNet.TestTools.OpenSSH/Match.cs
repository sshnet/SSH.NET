namespace Renci.SshNet.TestTools.OpenSSH
{
    public sealed class Match
    {
        public Match(string[] users, string[] addresses)
        {
            Users = users;
            Addresses = addresses;
        }

        public string[] Users { get; }

        public string[] Addresses { get; }

        public string? AuthenticationMethods { get; set; }

        public void WriteTo(TextWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write("Match ");

            if (Users.Length > 0)
            {
                writer.Write("User ");
                for (var i = 0; i < Users.Length; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(',');
                    }

                    writer.Write(Users[i]);
                }
            }

            if (Addresses.Length > 0)
            {
                writer.Write("Address ");
                for (var i = 0; i < Addresses.Length; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(',');
                    }

                    writer.Write(Addresses[i]);
                }
            }

            writer.WriteLine();

            if (AuthenticationMethods != null)
            {
                writer.WriteLine("    AuthenticationMethods " + AuthenticationMethods);
            }
        }
    }
}
