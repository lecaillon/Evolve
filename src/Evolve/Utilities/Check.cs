using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Evolve.Utilities
{
    /// <summary>
    ///     Static convenience methods to check that a method or a constructor is invoked with proper parameter or not.
    /// </summary>
    internal static class Check
    {
        private const string ArgumentIsEmpty = "The string cannot be empty.";
        private const string CollectionArgumentHasNullElement = "The collection must not contain any null element.";
        private const string FileNotFound = "The file does not exist.";

        /// <summary>
        ///     Ensures that the string passed as a parameter is neither null or empty.
        /// </summary>
        /// <param name="text"> The string to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The not null or empty string that was validated. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the string is null. </exception>
        /// <exception cref="ArgumentException"> Throws ArgumentException if the string is empty. </exception>
        public static string NotNullOrEmpty(string? text, string parameterName)
        {
            if (text is null)
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }
            else if (text.Trim().Length == 0)
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(ArgumentIsEmpty, parameterName);
            }

            return text;
        }

        /// <summary>
        ///     Ensures that an object <paramref name="reference"/> passed as a parameter is not null.
        /// </summary>
        /// <typeparam name="T"> The type of the reference to test. </typeparam>
        /// <param name="reference"> An object reference. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The non-null reference that was validated. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the reference is null. </exception>
        public static T NotNull<T>(T? reference, string parameterName) where T : class
        {
            if (reference is null)
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return reference;
        }

        /// <summary>
        ///     Ensures that a <paramref name="enumerable"/> does not contain a null element.
        /// </summary>
        /// <typeparam name="T"> The type of the enumerable to test. </typeparam>
        /// <param name="enumerable"> The enumerable to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The enumerable without null element that was validated. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the enumerable is null. </exception>
        /// <exception cref="ArgumentException"> Throws ArgumentException if the enumerable contains at least one null element. </exception>
        public static IEnumerable<T> HasNoNulls<T>(IEnumerable<T> enumerable, string parameterName) where T : class
        {
            NotNull(enumerable, parameterName);

            if (enumerable.Any(e => e is null))
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CollectionArgumentHasNullElement, parameterName);
            }

            return enumerable;
        }

        /// <summary>
        ///     Ensures that the specified file exists.
        /// </summary>
        /// <param name="filePath"> The full path of the file to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The full path of the file tested and found. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the path is null. </exception>
        /// <exception cref="ArgumentException"> Throws ArgumentException (with an inner FileNotFoundException) if the file is not found. </exception>
        public static string FileExists(string filePath, string parameterName)
        {
            NotNullOrEmpty(filePath, parameterName);

            if (!File.Exists(filePath))
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(FileNotFound, parameterName, new FileNotFoundException(FileNotFound, filePath));
            }

            return filePath;
        }
    }
}
