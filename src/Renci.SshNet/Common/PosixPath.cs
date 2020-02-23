using System;

namespace Renci.SshNet.Common
{
    internal class PosixPath
    {
        public string Directory { get; private set; }
        public string File { get; private set; }

        public static PosixPath CreateAbsoluteOrRelativeFilePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            var posixPath = new PosixPath();

            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1)
            {
                if (path.Length == 0)
                {
                    throw new ArgumentException("The path is a zero-length string.", "path");
                }

                posixPath.Directory = ".";
                posixPath.File = path;
            }
            else if (pathEnd == 0)
            {
                posixPath.Directory = "/";
                if (path.Length > 1)
                {
                    posixPath.File = path.Substring(pathEnd + 1);
                }
            }
            else
            {
                posixPath.Directory = path.Substring(0, pathEnd);
                if (pathEnd < path.Length - 1)
                {
                    posixPath.File = path.Substring(pathEnd + 1);
                }
            }

            return posixPath;
        }

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
        /// If path has a trailing slash, <see cref="GetFileName(string)"/> return a zero-length string.
        /// </para>
        /// </remarks>
        public static string GetFileName(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1)
                return path;
            if (pathEnd == path.Length - 1)
                return string.Empty;
            return path.Substring(pathEnd + 1);
        }

        /// <summary>
        /// Gets the directory name part of a given POSIX path.
        /// </summary>
        /// <param name="path">The POSIX path to get the directory name for.</param>
        /// <returns>
        /// The directory part of the specified <paramref name="path"/>, or <c>.</c> if <paramref name="path"/>
        /// does not contain any directory information.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
        public static string GetDirectoryName(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1)
                return ".";
            if (pathEnd == 0)
                return "/";
            if (pathEnd == path.Length - 1)
                return path.Substring(0, pathEnd);
            return path.Substring(0, pathEnd);
        }
    }
}
