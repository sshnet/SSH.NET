using System;

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
        /// <exception cref="NullReferenceException"><paramref name="path"/> is <c>null</c>.</exception>
        /// <remarks>
        /// <para>
        /// If <paramref name="path"/> contains no forward slash, then <paramref name="path"/>
        /// is returned.
        /// </para>
        /// <para>
        /// If path has a trailing slash, but <see cref="GetFileName(string)"/> return a zero-length string.
        /// </para>
        /// </remarks>
        public static string GetFileName(string path)
        {
            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1)
                return path;
            if (pathEnd == path.Length - 1)
                return string.Empty;
            return path.Substring(pathEnd + 1);
        }
    }
}
