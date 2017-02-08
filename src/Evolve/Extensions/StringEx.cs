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
    }
}
