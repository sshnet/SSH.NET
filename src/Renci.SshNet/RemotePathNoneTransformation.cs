﻿using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Performs no transformation.
    /// </summary>
    internal sealed class RemotePathNoneTransformation : IRemotePathTransformation
    {
        /// <summary>
        /// Returns the specified path without applying a transformation.
        /// </summary>
        /// <param name="path">The path to transform.</param>
        /// <returns>
        /// The specified path as is.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This transformation is recommended for servers that do not require any quoting to preserve the
        /// literal value of metacharacters, or when paths are guaranteed to never contain any such characters.
        /// </remarks>
        public string Transform(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return path;
        }
    }
}
