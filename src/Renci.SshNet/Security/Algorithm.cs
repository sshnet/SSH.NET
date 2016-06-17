namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents the abstract base class from which all implementations of algorithms must inherit.
    /// </summary>
    public abstract class Algorithm
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public abstract string Name { get; }
    }
}
