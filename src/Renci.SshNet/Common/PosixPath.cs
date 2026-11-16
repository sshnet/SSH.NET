#nullable enable
using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Represents a POSIX path.
    /// </summary>
    internal sealed class PosixPath
    {
        private PosixPath(string directory, string? file)
        {
            Directory = directory;
            File = file;
        }

        /// <summary>
        /// Gets the directory of the path.
        /// </summary>
        /// <value>
        /// The directory of the path.
        /// </value>
        public string Directory { get; }

        /// <summary>
        /// Gets the file part of the path.
        /// </summary>
        /// <value>
        /// The file part of the path, or <see langword="null"/> if the path represents a directory.
        /// </value>
        public string? File { get; }

        /// <summary>
        /// Create a <see cref="PosixPath"/> from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// A <see cref="PosixPath"/> created from the specified path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is empty ("").</exception>
        public static PosixPath CreateAbsoluteOrRelativeFilePath(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            string directory;
            string? file = null;

            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1)
            {
                if (path.Length == 0)
                {
                    throw new ArgumentException("The path is a zero-length string.", nameof(path));
                }

                directory = ".";
                file = path;
            }
            else if (pathEnd == 0)
            {
                directory = "/";

                if (path.Length > 1)
                {
                    file = path.Substring(pathEnd + 1);
                }
            }
            else
            {
                directory = path.Substring(0, pathEnd);
                if (pathEnd < path.Length - 1)
                {
                    file = path.Substring(pathEnd + 1);
                }
            }

            return new PosixPath(directory, file);
        }

        /// <summary>
        /// Gets the file name part of a given POSIX path.
        /// </summary>
        /// <param name="path">The POSIX path to get the file name for.</param>
        /// <returns>
        /// The file name part of <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>
        /// If <paramref name="path"/> contains no forward slash, then <paramref name="path"/>
        /// is returned.
        /// </para>
        /// <para>
        /// If path has a trailing slash, <see cref="GetFileName(string)"/> returns a zero-length string.
        /// </para>
        /// </remarks>
        public static string GetFileName(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1)
            {
                return path;
            }

            if (pathEnd == path.Length - 1)
            {
                return string.Empty;
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        public static string GetDirectoryName(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            var pathEnd = path.LastIndexOf('/');
            if (pathEnd == -1)
            {
                return ".";
            }

            if (pathEnd == 0)
            {
                return "/";
            }

            if (pathEnd == path.Length - 1)
            {
                return path.Substring(0, pathEnd);
            }

            return path.Substring(0, pathEnd);
        }
    }
}
