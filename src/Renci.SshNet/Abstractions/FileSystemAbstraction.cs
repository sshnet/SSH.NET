using System;
using System.Collections.Generic;
using System.IO;

namespace Renci.SshNet.Abstractions
{
    internal class FileSystemAbstraction
    {
        /// <summary>
        /// Returns an enumerable collection of file information that matches a search pattern.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="searchPattern">The search string to match against the names of files.</param>
        /// <returns>
        /// An enumerable collection of files that matches <paramref name="searchPattern"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="searchPattern"/> is <c>null</c>.</exception>
        /// <exception cref="DirectoryNotFoundException">The path represented by <paramref name="directoryInfo"/> does not exist or is not valid.</exception>
        public static IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo directoryInfo, string searchPattern)
        {
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo");

#if FEATURE_DIRECTORYINFO_ENUMERATEFILES
            return directoryInfo.EnumerateFiles(searchPattern);
#else
            return directoryInfo.GetFiles(searchPattern);
#endif
        }
    }
}
