using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    }
}
