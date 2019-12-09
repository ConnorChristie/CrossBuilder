using CrossBuilder.Deb;
using System.Linq;

namespace CrossBuilder
{
    public static class StringExtensions
    {
        public static char? TryGetValue(this string str, int index)
        {
            if (index < str.Length)
            {
                return str[index];
            }

            return null;
        }

        public static void SplitIntoTwo(this string str, char c, out string left, out string right)
        {
            left = str;
            right = "";

            var parts = str.Split(c);
            if (parts.Length > 1)
            {
                left = parts[0];
                right = parts[1];
            }
        }

        public static bool ContainsAndRetreive(this string str, string key, out int? value)
        {
            if (ContainsAndRetreive(str, key, out string strValue) && int.TryParse(strValue, out var intValue))
            {
                value = intValue;
                return true;
            }

            value = null;
            return false;
        }

        public static bool ContainsAndRetreive(this string str, string key, out string value)
        {
            if (str.StartsWith(key))
            {
                value = str.Substring(key.Length);
                return true;
            }

            value = null;
            return false;
        }

        public static Comparer ToComparisonOperator(this string op)
        {
            switch (op[0])
            {
                case '<':
                    if (op.ElementAtOrDefault(1) == '=')
                        return Comparer.LessEq;
                    if (op.ElementAtOrDefault(1) == '<')
                        return Comparer.Less;
                    return Comparer.LessEq;
                case '>':
                    if (op.ElementAtOrDefault(1) == '=')
                        return Comparer.GreaterEq;
                    if (op.ElementAtOrDefault(1) == '>')
                        return Comparer.Greater;
                    return Comparer.GreaterEq;
                case '=':
                    return Comparer.Equals;
                case '!':
                    if (op.ElementAtOrDefault(1) == '=')
                        return Comparer.NotEquals;
                    break;
            }

            return Comparer.Equals;
        }
    }
}
