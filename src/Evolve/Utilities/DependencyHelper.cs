using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EvolveDb.Utilities
{
    internal static class DependencyHelper
    {
        private static Regex? _depRegex = null;
        internal static IEnumerable<string> Get(string content, string option)
        {
            if (_depRegex is null)
            {
                _depRegex = new Regex(
                    $@"{option}\s*=(\s*(?<deps>[^\|]+)\s*\|?)+",
                    RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.Compiled
                );
            }
            var matches = _depRegex.Match(content);
            if (!matches.Success)
            {
                yield break;
            }
            foreach (var capture in matches.Groups["deps"].Captures)
            {
                yield return capture.ToString();
            }
        }
    }
}
