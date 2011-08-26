using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet
{
    /// <summary>
    /// Collection of different extension method specific for .NET 4.0
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        ///   <c>true</c> if the value parameter is null or System.String.Empty, or if value consists exclusively of white-space characters; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}
