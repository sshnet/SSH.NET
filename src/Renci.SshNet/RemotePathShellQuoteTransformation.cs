using System;
using System.Text;

namespace Renci.SshNet
{
    /// <summary>
    /// Quotes a path in a way to be suitable to be used with a shell-based server.
    /// </summary>
    internal class RemotePathShellQuoteTransformation : IRemotePathTransformation
    {
        /// <summary>
        /// Quotes a path in a way to be suitable to be used with a shell-based server.
        /// </summary>
        /// <param name="path">The path to transform.</param>
        /// <returns>
        /// A quoted path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
        /// <remarks>
        /// <para>
        /// If <paramref name="path"/> contains a single-quote, that character is embedded
        /// in quotation marks (eg. "'"). Sequences of single-quotes are grouped in a single
        /// pair of quotation marks.
        /// </para>
        /// <para>
        /// An exclamation mark in <paramref name="path"/> is escaped with a backslash. This is
        /// necessary because C Shell interprets it as a meta-character for history substitution
        /// even when enclosed in single quotes or quotation marks.
        /// </para>
        /// <para>
        /// All other characters are enclosed in single quotes. Sequences of such characters are grouped
        /// in a single pair of single quotes.
        /// </para>
        /// <para>
        /// References:
        /// <list type="bullet">
        ///   <item>
        ///     <description><a href="http://pubs.opengroup.org/onlinepubs/7908799/xcu/chap2.html">Shell Command Language</a></description>
        ///   </item>
        ///   <item>
        ///     <description><a href="https://earthsci.stanford.edu/computing/unix/shell/specialchars.php">Unix C-Shell special characters and their uses</a></description>
        ///   </item>
        ///   <item>
        ///     <description><a href="https://docstore.mik.ua/orelly/unix3/upt/ch27_13.htm">Differences Between Bourne and C Shell Quoting</a></description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Original</term>
        ///     <term>Transformed</term>
        ///   </listheader>
        ///   <item>
        ///     <term>/var/log/auth.log</term>
        ///     <term>'/var/log/auth.log'</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/mp3/Guns N' Roses</term>
        ///     <term>'/var/mp3/Guns N'"'"' Roses'</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/garbage!/temp</term>
        ///     <term>'/var/garbage'\!'/temp'</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/would be 'kewl'!, not?</term>
        ///     <term>'/var/would be '"'"'kewl'"'"\!', not?'</term>
        ///   </item>
        ///   <item>
        ///     <term></term>
        ///     <term>''</term>
        ///   </item>
        ///   <item>
        ///     <term>Hello &quot;World&quot;</term>
        ///     <term>'Hello "World"'</term>
        ///   </item>
        /// </list>
        /// </example>
        public string Transform(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            // result is at least value and (likely) leading/trailing single-quotes
            var sb = new StringBuilder(path.Length + 2);
            var state = ShellQuoteState.Unquoted;

            foreach (var c in path)
            {
                switch (c)
                {
                    case '\'':
                        // embed a single-quote in quotes
                        switch (state)
                        {
                            case ShellQuoteState.Unquoted:
                                // Start quoted string
                                sb.Append('"');
                                break;
                            case ShellQuoteState.Quoted:
                                // Continue quoted string
                                break;
                            case ShellQuoteState.SingleQuoted:
                                // Close single-quoted string
                                sb.Append('\'');
                                // Start quoted string
                                sb.Append('"');
                                break;
                        }
                        state = ShellQuoteState.Quoted;
                        break;
                    case '!':
                        // In C-Shell, an exclamatation point can only be protected from shell interpretation
                        // when escaped by a backslash
                        // Source:
                        // https://earthsci.stanford.edu/computing/unix/shell/specialchars.php

                        switch (state)
                        {
                            case ShellQuoteState.Unquoted:
                                sb.Append('\\');
                                break;
                            case ShellQuoteState.Quoted:
                                // Close quoted string
                                sb.Append('"');
                                sb.Append('\\');
                                break;
                            case ShellQuoteState.SingleQuoted:
                                // Close single quoted string
                                sb.Append('\'');
                                sb.Append('\\');
                                break;
                        }
                        state = ShellQuoteState.Unquoted;
                        break;
                    default:
                        switch (state)
                        {
                            case ShellQuoteState.Unquoted:
                                // Start single-quoted string
                                sb.Append('\'');
                                break;
                            case ShellQuoteState.Quoted:
                                // Close quoted string
                                sb.Append('"');
                                // Start single-quoted string
                                sb.Append('\'');
                                break;
                            case ShellQuoteState.SingleQuoted:
                                // Continue single-quoted string
                                break;
                        }
                        state = ShellQuoteState.SingleQuoted;
                        break;
                }

                sb.Append(c);
            }

            switch (state)
            {
                case ShellQuoteState.Unquoted:
                    break;
                case ShellQuoteState.Quoted:
                    // Close quoted string
                    sb.Append('"');
                    break;
                case ShellQuoteState.SingleQuoted:
                    // Close single-quoted string
                    sb.Append('\'');
                    break;
            }

            if (sb.Length == 0)
            {
                sb.Append("''");
            }

            return sb.ToString();
        }

        private enum ShellQuoteState
        {
            Unquoted = 1,
            SingleQuoted = 2,
            Quoted = 3
        }
    }
}
