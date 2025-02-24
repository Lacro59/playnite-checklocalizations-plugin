using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonPluginsShared.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveDiacritics(this string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    _ = stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }


        public static string RemoveWhiteSpace(this string text)
        {
            return Regex.Replace(text, @"s", "");
        }


        public static bool IsEqual(this string source, string text, bool Normalize = false)
        {
            try
            {
                return string.IsNullOrEmpty(source) || string.IsNullOrEmpty(text)
                    ? false
                    : Normalize
                    ? PlayniteTools.NormalizeGameName(source).Trim().ToLower() == PlayniteTools.NormalizeGameName(text).Trim().ToLower()
                    : source.Trim().ToLower() == text.Trim().ToLower();
            }
            catch(Exception ex)
            {
                Common.LogError(ex, false);
                return false;
            }
        }
    }
}
