using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sayer.Strings
{
    public static class StringExtensions
    {
        /// <summary>
        /// Attempts to convert the string to the desired type, using the invariant culture
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="text">The string to convert</param>
        /// <param name="result">The result of the conversion, or default(T) if this method returns false</param>
        /// <returns>true if the text was able to be converted, false otherwise</returns>
        public static bool TryParse<T>(this string text, out T result)
        {
            return TryParse(text, false, out result);
        }

        /// <summary>
        /// Attempts to convert the string to the desired type, using the invariant culture
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="text">The string to convert</param>
        /// <param name="defaultForNullOrEmpty">
        /// If true, and the input string is null or empty, this method will return true
        /// and default(T) will be the result of the conversion
        /// </param>
        /// <param name="result">The result of the conversion, or default(T) if this method returns false</param>
        /// <returns>true if the text was able to be converted, false otherwise</returns>
        public static bool TryParse<T>(this string text, bool defaultForNullOrEmpty, out T result)
        {
            try
            {
                result = Parse<T>(text, defaultForNullOrEmpty);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Converts the string to the desired type, using the invariant culture
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="text">The string to convert</param>
        /// <returns>The result of the conversion</returns>
        public static T Parse<T>(this string text)
        {
            return Parse<T>(text, false);
        }

        /// <summary>
        /// Converts the string to the desired type, using the invariant culture
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="text">The string to convert</param>
        /// <param name="defaultForNullOrEmpty">If true, and the input string is null or empty, default(T) will be returned</param>
        /// <returns>The result of the conversion</returns>
        public static T Parse<T>(this string text, bool defaultForNullOrEmpty)
        {
            if (defaultForNullOrEmpty && string.IsNullOrEmpty(text))
            {
                return default;
            }

            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(text);
        }

        /// <summary>
        /// Converts a string to a dictionary, given separator tokens. For example, if the element separator token
        /// is ';' and the assignmentToken is '=', the following text could be convert to a dictionary where
        /// the key is a number and the value is a double: "1=3.6;7=4.2"
        /// </summary>
        /// <typeparam name="K">the key type</typeparam>
        /// <typeparam name="V">the value type</typeparam>
        /// <param name="text">The text to convert.</param>
        /// <param name="elementSeparatorToken"></param>
        /// <param name="assignmentToken"></param>
        /// <returns>The dictionary</returns>
        public static Dictionary<K, V> ToDictionary<K, V>(this string text, char elementSeparatorToken, char assignmentToken = '=') =>
            ToDictionary<K, V, Dictionary<K, V>>(text, new char[] { elementSeparatorToken }, new char[] { assignmentToken }, capacity => new Dictionary<K, V>(capacity));

        /// <summary>
        /// Converts a string to a dictionary, given separator tokens. For example, if the element separator token
        /// is ';' and the assignmentToken is '=', the following text could be convert to a dictionary where
        /// the key is a number and the value is a double: "1=3.6;7=4.2"
        /// </summary>
        /// <typeparam name="K">the key type</typeparam>
        /// <typeparam name="V">the value type</typeparam>
        /// <param name="text">The text to convert.</param>
        /// <param name="elementSeparatorToken"></param>
        /// <param name="assignmentToken"></param>
        /// <param name="factory">delegate that returns an IDictionary, given </param>
        /// <returns>The dictionary</returns>
        public static DictionaryType ToDictionary<K, V, DictionaryType>(this string text, char[] elementSeparator, char[] assignmentSeparator, Func<int, DictionaryType> factory) where DictionaryType : IDictionary<K, V>
        {
            string[] split = text.Split(elementSeparator);
            DictionaryType result = factory(split.Length);

            foreach (string element in split)
            {
                string[] components = element.Split(assignmentSeparator);

                switch (components.Length)
                {
                    case 1:
                        result.Add(components[0].Parse<K>(), default);
                        break;
                    case 2:
                        result.Add(components[0].Parse<K>(), components[1].Parse<V>());
                        break;
                    default:
                        throw new ArgumentException($"Invalid input string {text}");
                }
            }

            return result;
        }
    }
}
