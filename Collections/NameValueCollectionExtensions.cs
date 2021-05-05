using Sayer.Strings;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sayer.Collections
{
    /// <summary>
    /// Extensions for NameValueCollection
    /// </summary>
    public static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Allows for storing types other than strings in a NameValueCollection. Note that under the covers,
        /// a string is stored regardless. A conversion is used to stringify the provided value.
        /// </summary>
        /// <typeparam name="T">The type of object being stored</typeparam>
        /// <param name="nameValueCollection">The collection</param>
        /// <param name="key">The key with which the value should be associated</param>
        /// <param name="value">The value to store. May be null.</param>
        public static void Add<T>(this NameValueCollection nameValueCollection, string key, T value)
        {
            nameValueCollection.Add(key, value == null ? null : TypeDescriptor.GetConverter(typeof(T)).ConvertToInvariantString(value));
        }

        /// <summary>
        /// Allows for retrieving types other than strings in a NameValueCollection. Note that under the covers,
        /// a string is stored regardless. A conversion is used to return the desired type.
        /// </summary>
        /// <typeparam name="T">The type of object being retrieved</typeparam>
        /// <param name="nameValueCollection">The collection</param>
        /// <param name="key">The key with the associated value to look up</param>
        /// <param name="value">If there is a value associated with key, this will be set to that value. Otherwise will be set to default(T).</param>
        /// <returns>true if a value was associated with the key, otherwise false</returns>
        public static bool TryGet<T>(this NameValueCollection nameValueCollection, string key, out T value)
        {
            string text = nameValueCollection.Get(key);
            return text.TryParse(false, out value);
        }

        /// <summary>
        /// Allows for retrieving types other than strings in a NameValueCollection. Note that under the covers,
        /// a string is stored regardless. A conversion is used to return the desired type.
        /// </summary>
        /// <typeparam name="T">The type of object being retrieved</typeparam>
        /// <param name="nameValueCollection">The collection</param>
        /// <param name="key">The key with the associated value to look up</param>
        /// <param name="defaultValue">The value to return if not present (or null) in the collection. May be null.</param>
        /// <returns>The value associated with the provided key, or, if not present (or null), the default value.</returns>
        public static T Get<T>(this NameValueCollection nameValueCollection, string key, T defaultValue)
        {
            if (TryGet(nameValueCollection, key, out T result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}
