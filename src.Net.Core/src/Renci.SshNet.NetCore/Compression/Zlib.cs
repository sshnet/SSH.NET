namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents "zlib" compression implementation
    /// </summary>
    internal class Zlib : Compressor
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "zlib"; }
        }

        /// <summary>
        /// Initializes the algorithm
        /// </summary>
        /// <param name="session">The session.</param>
        public override void Init(Session session)
        {
            base.Init(session);
            IsActive = true;
        }
    }
}