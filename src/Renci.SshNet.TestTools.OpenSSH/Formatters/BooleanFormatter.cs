namespace Renci.SshNet.TestTools.OpenSSH.Formatters
{
    internal class BooleanFormatter
    {
        public string Format(bool value)
        {
            return value ? "yes" : "no";
        }

        public string Format(bool? value, bool defaultValue)
        {
            if (value.HasValue)
            {
                return Format(value.Value);
            }

            return Format(defaultValue);
        }
    }
}
