using System;

namespace Evolve
{
    public static class StringEx
    {
        /// <summary>
        ///     Truncates a string to be no longer than a certain length.
        /// </summary>
        public static string TruncateWithEllipsis(this string s, int maxLength)
        {
            const string Ellipsis = "...";

            if (Ellipsis.Length > maxLength)
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "Max length must be at least as long as ellipsis.");

            return (s.Length > maxLength) ? s.Substring(0, maxLength - Ellipsis.Length) + Ellipsis : s;
        }

        /// <summary>
        ///     Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="s"> The string to test. </param>
        /// <returns> True if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters. </returns>
        public static bool IsNullOrWhiteSpace(this string s)
        {
#if NET35
            if (s == null) return true;
            return string.IsNullOrEmpty(s.Trim());
#else
            return string.IsNullOrWhiteSpace(s);
#endif
        }

        /// <summary>
        ///     Returns a value indicating whether the specified String object occurs within this string.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"> The String object to seek. </param>
        /// <param name="comp"> One of the enumeration values that specifies how the strings will be compared. </param>
        /// <returns> True if the value parameter occurs within this string, otherwise, false. </returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
