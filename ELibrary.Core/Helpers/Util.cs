using System.Text.RegularExpressions;

namespace ELibrary.Core.Helpers
{
    public static class Util
    {
        public static string SanitizeForComparison(this string input, bool discardInitials = false)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            string alphanumeric = Regex.Replace(input, "[^a-zA-Z0-9 ]", "");
            if (!discardInitials)
            {
                return alphanumeric;
            }
            string noInitials = Regex.Replace(alphanumeric, @"\b[a-zA-Z]\b\s*", "");
            return Regex.Replace(noInitials, @"\s+", "");
        }
        
        public static string TruncateText(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length <= maxLength) return text;

            var truncated = text.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                truncated = truncated.Substring(0, lastSpace);
            }
            return truncated + "...";
        }
    }
}
