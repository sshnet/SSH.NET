namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal class SubsystemFormatter
    {
        public string Format(Subsystem subsystem)
        {
            return subsystem.Name + " " + subsystem.Command;
        }
    }
}
