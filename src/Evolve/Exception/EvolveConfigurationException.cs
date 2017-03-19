using System;

namespace Evolve
{
    public class EvolveConfigurationException : EvolveException
    {
        private const string EvolveConfigurationError = "Evolve configuration error: ";

        public EvolveConfigurationException(string message) : base(EvolveConfigurationError + FirstLetterToLower(message)) { }

        public EvolveConfigurationException(string message, Exception innerException) : base(EvolveConfigurationError + FirstLetterToLower(message), innerException) { }

        private static string FirstLetterToLower(string str)
        {
            if (str == null)
            {
                return "";
            }

            if (str.Length > 1)
            {
                return Char.ToLowerInvariant(str[0]) + str.Substring(1);
            }

            return str.ToLowerInvariant();
        }
    }
}
