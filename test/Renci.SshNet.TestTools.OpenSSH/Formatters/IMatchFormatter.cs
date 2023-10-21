namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal interface IMatchFormatter : IFormatter<Match>
    {
        void Format(Match match, TextWriter writer);
    }
}
