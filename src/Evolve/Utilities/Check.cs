using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Evolve.Utilities
{
    /// <summary>
    /// Static convenience methods to check that a method or a constructor is invoked with proper parameter or not.
    /// </summary>
    public static class Check
    {
        private const string ArgumentIsEmpty = "The string cannot be empty.";
        private const string CollectionArgumentIsEmpty = "The collection must contain at least one element.";
        private const string CollectionArgumentHasNullElement = "The collection must not contain any null element.";
        private const string FileNotFound = "The file does not exist.";
        private const string DirectoryNotFound = "Directory not found at: {0}";

        /// <summary>
        ///     Ensures that the string passed as a parameter is neither null or empty.
        /// </summary>
        /// <param name="text"> The string to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The not null or empty string that was validated. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the string is null. </exception>
        /// <exception cref="ArgumentException"> Throws ArgumentException if the string is empty. </exception>
        public static string NotNullOrEmpty(string text, string parameterName)
        {
            Exception e = null;
            if (ReferenceEquals(text, null))
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (text.Trim().Length == 0)
            {
                e = new ArgumentException(ArgumentIsEmpty, parameterName);
            }

            if (e != null)
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return text;
        }

        /// <summary>
        ///     Ensures that a string is not empty, but can be null.
        /// </summary>
        /// <param name="text"> The string to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The non empty string that was validated. </returns>
        /// <exception cref="ArgumentException"> Throws ArgumentException if the string is empty. </exception>
        public static string NullableButNotEmpty(string text, string parameterName)
        {
            if (!ReferenceEquals(text, null) && (text.Length == 0))
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
        public static T NotNull<T>(T reference, string parameterName)
        {
            if (ReferenceEquals(reference, null))
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return reference;
        }

        /// <summary>
        ///     Ensures that a <paramref name="collection"/> contains at least one element.
        /// </summary>
        /// <typeparam name="T"> The type of the collection to test. </typeparam>
        /// <param name="collection"> The collection to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The non-empty collection that was validated. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the collection is null. </exception>
        /// <exception cref="ArgumentException"> Throws ArgumentException when the collection has no element. </exception>
        public static ICollection<T> NotEmpty<T>(ICollection<T> collection, string parameterName)
        {
            NotNull(collection, parameterName);

            if (collection.Count == 0)
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CollectionArgumentIsEmpty, parameterName);
            }

            return collection;
        }

        /// <summary>
        ///     Ensures that a <paramref name="enumerable"/> contains at least one element.
        /// </summary>
        /// <typeparam name="T"> The type of the enumerable to test. </typeparam>
        /// <param name="enumerable"> The enumerable to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The non-empty enumerable that was validated. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the enumerable is null. </exception>
        /// <exception cref="ArgumentException"> Throws ArgumentException when the enumerable has no element. </exception>
        public static IEnumerable<T> NotEmpty<T>(IEnumerable<T> enumerable, string parameterName)
        {
            NotNull(enumerable, parameterName);

            if (!enumerable.Any())
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CollectionArgumentIsEmpty, parameterName);
            }

            return enumerable;
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

            if (enumerable.Any(e => e == null))
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

            if(!File.Exists(filePath))
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(FileNotFound, parameterName, new FileNotFoundException(FileNotFound, filePath));
            }

            return filePath;
        }

        /// <summary>
        ///     Ensures that the specified directory exists.
        /// </summary>
        /// <param name="path"> The full path of the directory to test. </param>
        /// <param name="parameterName"> The name of the parameter to test. </param>
        /// <returns> The full path of the directory tested and found. </returns>
        /// <exception cref="ArgumentNullException"> Throws ArgumentNullException if the path is null. </exception>
        /// <exception cref="ArgumentException"> Throws ArgumentException (with an inner DirectoryNotFoundException) if the directory is not found. </exception>
        public static string DirectoryExists(string path, string parameterName)
        {
            NotNullOrEmpty(path, parameterName);

            if (!Directory.Exists(path))
            {
                NotNullOrEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(string.Format(DirectoryNotFound, path), parameterName,
                                            new DirectoryNotFoundException(path));
            }

            return path;
        }
    }
}
