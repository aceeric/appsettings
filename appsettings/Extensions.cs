using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppSettings
{
    static class Extensions
    {
        /// <summary>
        /// Splits the string on the first occurrance of the split character. E.g. "foo=bar=baz".SplitFirst('=') will split
        /// into "foo" and "bar=baz"
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>

        public static string [] SplitFirst(this string Value, char SplitChar)
        {
            int Idx = Value.IndexOf(SplitChar);
            if (Idx != -1)
            {
                return new string[] { Value.Substring(0, Idx), Value.Length > Idx + 1 ? Value.Substring(Idx+1) : string.Empty};
            }
            else
            {
                return new string[] { Value };
            }
        }

        /// <summary>
        /// Replaces every character in the input with the replacement string. E.g. Stuff("hello", "-") returns: "-----"
        /// </summary>
        /// <param name="In"></param>
        /// <param name="Repl"></param>
        /// <returns></returns>

        public static string Stuff(this string In, string Repl)
        {
            return Regex.Replace(In, ".", Repl);
        }
    }
}
