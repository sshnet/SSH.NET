namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents the abstract base class from which all implementations of algorithms must inherit.
    /// </summary>
#pragma warning disable S1694 // An abstract class should have both abstract and concrete methods
    public abstract class Algorithm
#pragma warning restore S1694 // An abstract class should have both abstract and concrete methods
    {
        /// <summary>
        /// Gets the algorithm name.
        /// </summary>
        public abstract string Name { get; }
    }
}
