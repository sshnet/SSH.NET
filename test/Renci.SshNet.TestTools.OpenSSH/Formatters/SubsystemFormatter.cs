namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal sealed class SubsystemFormatter : IFormatter<Subsystem>
    {
        public string Format(Subsystem subsystem)
        {
            return subsystem.Name + " " + subsystem.Command;
        }
    }
}
