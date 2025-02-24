using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace CommonPluginsShared.Extensions
{
    // https://stackoverflow.com/a/60671815

    /// <summary>
    /// A class to hold extension methods for C# Lists 
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Convert a list of Type T to a CSV
        /// </summary>
        /// <typeparam name="T">The type of the object held in the list</typeparam>
        /// <param name="items">The list of items to process</param>
        /// <param name="delimiter">Specify the delimiter, default is ,</param>
        /// <returns></returns>
        public static string ToCsv<T>(this List<T> items, bool orderBy = true, string delimiter = ",", bool noHeader = false, List<string> header = null)
        {
            Type itemType = typeof(T);
            IEnumerable<PropertyInfo> props = orderBy
                ? itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name)
                : (IEnumerable<PropertyInfo>)itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            StringBuilder csv = new StringBuilder();

            // Write Headers
            if (!noHeader)
            {
                _ = header?.Count > 0 ? csv.AppendLine(string.Join(delimiter, header)) : csv.AppendLine(string.Join(delimiter, props.Select(p => p.Name)));
            }

            // Write Rows
            foreach (T item in items)
            {
                // Write Fields
                _ = csv.AppendLine(string.Join(delimiter, props.Select(p => GetCsvFieldasedOnValue(p, item))));
            }

            return csv.ToString();
        }

        /// <summary>
        /// Provide generic and specific handling of fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static object GetCsvFieldasedOnValue<T>(PropertyInfo p, T item)
        {
            string value;
            try
            {
                value = p.GetValue(item, null)?.ToString();
                if (value == null)
                {
                    return "NULL";  // Deal with nulls
                }

                if (value.Trim().Length == 0)
                {
                    return ""; // Deal with spaces and blanks
                }

                // Guard strings with "s, they may contain the delimiter!
                if (p.PropertyType == typeof(string))
                {
                    value = string.Format("\"{0}\"", value);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return value;
        }
    }
}
