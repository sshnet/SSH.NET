namespace Renci.SshClient.Security
{
    public abstract class Compression : Algorithm
    {
        private class CompressionNone : Compression
        {

            public override string Name
            {
                get { return "none"; }
            }
        }

        static Compression()
        {
            Compression.None = new CompressionNone
            {
            };
        }

        public static Compression None { get; private set; }
    }
}
