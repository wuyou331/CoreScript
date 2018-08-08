using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreScript
{
    public static class ParserExtension
    {
        public static string Text(this IEnumerable<char> item)
        {
            return new string(item.ToArray());
        }
 
        public static string Text(this IEnumerable<string> item)
        {
            var arr = item as string[] ?? item.ToArray();
            if ( arr.Length == 1) return arr[0];

            var size = arr.Select(it => it?.Length ?? 0).Sum();
            var sb = new StringBuilder(size);
            foreach (var s in arr)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }
    }
}