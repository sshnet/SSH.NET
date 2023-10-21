namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal interface IFormatter<in T>
    {
        string Format(T value);
    }
}
