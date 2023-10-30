﻿using System;
using System.Text;

namespace Renci.SshNet
{
    /// <summary>
    /// Encloses a path in double quotes, and escapes any embedded double quote with a backslash.
    /// </summary>
    internal sealed class RemotePathDoubleQuoteTransformation : IRemotePathTransformation
    {
        /// <summary>
        /// Encloses a path in double quotes, and escapes any embedded double quote with a backslash.
        /// </summary>
        /// <param name="path">The path to transform.</param>
        /// <returns>
        /// The transformed path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <example>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Original</term>
        ///     <term>Transformed</term>
        ///   </listheader>
        ///   <item>
        ///     <term>/var/log/auth.log</term>
        ///     <term>&quot;/var/log/auth.log&quot;</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/mp3/Guns N' Roses</term>
        ///     <term>&quot;/var/mp3/Guns N' Roses&quot;</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/garbage!/temp</term>
        ///     <term>&quot;/var/garbage!/temp&quot;</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/would be 'kewl'!/not?</term>
        ///     <term>&quot;/var/would be 'kewl'!, not?&quot;</term>
        ///   </item>
        ///   <item>
        ///     <term></term>
        ///     <term>&quot;&quot;</term>
        ///   </item>
        ///   <item>
        ///     <term>Hello &quot;World&quot;</term>
        ///     <term>&quot;Hello \&quot;World&quot;</term>
        ///   </item>
        /// </list>
        /// </example>
        public string Transform(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var transformed = new StringBuilder(path.Length);

            _ = transformed.Append('"');

            foreach (var c in path)
            {
                if (c == '"')
                {
                    _ = transformed.Append('\\');
                }

                _ = transformed.Append(c);
            }

            _ = transformed.Append('"');

            return transformed.ToString();
        }
    }
}
