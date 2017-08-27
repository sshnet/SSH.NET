namespace Renci.SshNet
{
    /// <summary>
    /// Performs no transformation.
    /// </summary>
    internal class RemotePathNoneTransformation : IRemotePathTransformation
    {
        /// <summary>
        /// Returns the specified path without applying a transformation.
        /// </summary>
        /// <param name="path">The path to transform.</param>
        /// <returns>
        /// The specified path as is.
        /// </returns>
        public string Transform(string path)
        {
            return path;
        }
    }
}
