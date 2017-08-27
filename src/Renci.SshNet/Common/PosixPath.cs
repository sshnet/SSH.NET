namespace Renci.SshNet.Common
{
    internal class PosixPath
    {
        /// <summary>
        /// Gets the file name part of a given POSIX path.
        /// </summary>
        /// <param name="path">The POSIX path to get the file name for.</param>
        /// <returns>
        /// The file name part of <paramref name="path"/>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="path"/> contains no forward slash or has a trailing
        /// forward slash, then <paramref name="path"/> is returned.
        /// </remarks>
        public static string GetFileName(string path)
        {
            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1 || pathEnd == path.Length - 1)
                return path;
            return path.Substring(pathEnd + 1);
        }
    }
}
