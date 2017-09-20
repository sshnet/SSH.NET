namespace Renci.SshNet
{
    /// <summary>
    /// Provides access to built-in remote path transformations.
    /// </summary>
    /// <remarks>
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
    ///   <item>
    ///     <description><a href="https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/">Everyone quotes command line arguments the wrong way</a></description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static class RemotePathTransformation
    {
        private static readonly IRemotePathTransformation ShellQuoteTransformation = new RemotePathShellQuoteTransformation();
        private static readonly IRemotePathTransformation NoneTransformation = new RemotePathNoneTransformation();
        private static readonly IRemotePathTransformation DoubleQuoteTransformation = new RemotePathDoubleQuoteTransformation();

        /// <summary>
        /// Quotes a path in a way to be suitable to be used with a shell-based server.
        /// </summary>
        /// <returns>
        /// A quoted path.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If a path contains a single-quote, that character is embedded in quotation marks (eg. "'").
        /// Sequences of single-quotes are grouped in a single pair of quotation marks.
        /// </para>
        /// <para>
        /// An exclamation mark in a path is escaped with a backslash. This is necessary because C Shell
        /// interprets it as a meta-character for history substitution even when enclosed in single quotes
        ///  or quotation marks.
        /// </para>
        /// <para>
        /// All other characters are enclosed in single quotes. Sequences of such characters are grouped
        /// in a single pair of single quotes.
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
        public static IRemotePathTransformation ShellQuote
        {
            get { return ShellQuoteTransformation; }
        }

        /// <summary>
        /// Performs no transformation.
        /// </summary>
        /// <remarks>
        /// Recommended for servers that do not require any character to be escaped or enclosed in quotes,
        /// or when paths are guaranteed to never contain any special characters (such as #, &quot;, ', $, ...).
        /// </remarks>
        public static IRemotePathTransformation None
        {
            get { return NoneTransformation; }
        }

        /// <summary>
        /// Encloses a path in double quotes, and escapes any embedded double quote with a backslash.
        /// </summary>
        /// <value>
        /// A transformation that encloses a path in double quotes, and escapes any embedded double quote with
        /// a backslash.
        /// </value>
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
        ///     <term>/var/would be 'kewl'!, not?</term>
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
        public static IRemotePathTransformation DoubleQuote
        {
            get { return DoubleQuoteTransformation; }
        }
    }
}
