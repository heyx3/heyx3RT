using System;
using System.Globalization;
using System.Linq;

namespace ObjLoader.Loader.Common
{
    public static class StringExtensions
    {
        public static float ParseInvariantFloat(this string floatString)
        {
            return float.Parse(floatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static int ParseInvariantInt(this string intString)
        {
            return int.Parse(intString, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static bool EqualsInvariantCultureIgnoreCase(this string str, string s)
        {
            return str.Equals(s, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return ReferenceEquals(str, null) ||
				   str.All(c => (c == ' ' || c == '\t' || c == '\n' || c == '\r'));
		}
    }
}