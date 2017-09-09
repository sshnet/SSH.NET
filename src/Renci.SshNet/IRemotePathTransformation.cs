namespace Renci.SshNet
{
    /// <summary>
    /// Represents a transformation that can be applied to a remote path.
    /// </summary>
    public interface IRemotePathTransformation
    {
        /// <summary>
        /// Transforms the specified remote path.
        /// </summary>
        /// <param name="path">The path to transform.</param>
        /// <returns>
        /// The transformed path.
        /// </returns>
        string Transform(string path);
    }


}
