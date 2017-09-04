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
        /// <remarks>
        /// This transformation is recommended for servers that do not require any quoting to preserve the
        /// literal value of metacharacters, or when paths are guaranteed to never contain any such characters.
        /// </remarks>
        public string Transform(string path)
        {
            return path;
        }
    }
}
