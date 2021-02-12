using System;
using System.Collections.Generic;

namespace Evolve
{
    internal static class MiscEx
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
        public static bool IsNullOrWhiteSpace(this string? s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        ///     Returns the value of the given dictionary key or default if not found.
        /// </summary>
        public static TV? GetValue<TK, TV>(this IDictionary<TK, TV>? dict, TK key, TV defaultValue = default)
        {
            return dict is null 
                ? defaultValue 
                : dict.TryGetValue(key, out TV value) 
                    ? value 
                    : defaultValue;
        }
    }
}
