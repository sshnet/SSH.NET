namespace Renci.SshNet
{
    /// <summary>
    /// Allow access to built-in remote path transformations.
    /// </summary>
    public static class RemotePathTransformation
    {
        private static readonly IRemotePathTransformation QuoteTransformation = new RemotePathQuoteTransformation();
        private static readonly IRemotePathTransformation NoneTransformation = new RemotePathNoneTransformation();

        /// <summary>
        /// Quotes a path in a way to be suitable to be used with a shell.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the path contains a single-quote, that character is embedded in quotation marks.
        /// Sequences of single-quotes are grouped in a single pair of quotation marks.
        /// </para>
        /// <para>
        /// An exclamation mark (!) is escaped with a backslash, because the C shell would otherwise
        /// interprete it as a meta-character for history substitution. It does this even if it's
        /// enclosed in single-quotes or quotation marks, unless escaped with a backslash (\).
        /// </para>
        /// <para>
        /// All other character are enclosed in single-quotes, and grouped in a single pair of
        /// single quotes where contiguous.
        /// </para>
        /// </remarks>
        /// <example>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Original</term>
        ///     <term>Quoted</term>
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
        ///     <term>'/var/garbage\!/temp'</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/garbage!/temp</term>
        ///     <term>'/var/garbage'\!'/temp'</term>
        ///   </item>
        ///   <item>
        ///     <term>/var/would be 'kewl'!/not?</term>
        ///     <term>'/var/would be '"'"'kewl'"'"\!'/not?'</term>
        ///   </item>
        ///   <item>
        ///     <term>!ignore!</term>
        ///     <term>\!'ignore'\!</term>
        ///   </item>
        ///   <item>
        ///     <term></term>
        ///     <term>''</term>
        ///   </item>
        /// </list>
        /// </example>
        public static IRemotePathTransformation Quote
        {
            get { return QuoteTransformation; }
        }

        /// <summary>
        /// Performs no transformation.
        /// </summary>
        /// <remarks>
        /// This transformation should be used for servers that do not support escape sequences in paths
        /// or paths enclosed in quotes, or would preserve the escape characters or quotes in the path that
        /// is handed off to the IO layer. This is recommended for servers that are not shell-based.
        /// </remarks>
        public static IRemotePathTransformation None
        {
            get { return NoneTransformation; }
        }
    }
}
