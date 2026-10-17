namespace Renci.SshNet
{
    /// <summary>
    /// Represents a transformation that can be applied to a remote path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A remote path transformation is used by <see cref="ScpClient"/> to encode a remote path before it
    /// is embedded in the <c>scp</c> command that is sent to the server. The correct transformation
    /// depends on the server: a shell-based server requires the path to be quoted or escaped according to
    /// its shell's rules, whereas a non-shell-based server uses the path literally and requires no
    /// transformation (see <see cref="RemotePathTransformation.None"/>).
    /// </para>
    /// <para>
    /// See <see cref="RemotePathTransformation"/> for the implementations supplied with SSH.NET. On a
    /// shell-based server, choosing a transformation that does not match the shell can leave a crafted
    /// path able to execute as a command on the server; on a non-shell-based server, applying quoting or
    /// escaping corrupts the path. A transformation should therefore be selected deliberately for the
    /// target server and the trust placed in the supplied paths.
    /// </para>
    /// </remarks>
    public interface IRemotePathTransformation
    {
        /// <summary>
        /// Transforms the specified remote path.
        /// </summary>
        /// <param name="path">The path to transform.</param>
        /// <returns>
        /// The transformed path.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        string Transform(string path);
    }
}
