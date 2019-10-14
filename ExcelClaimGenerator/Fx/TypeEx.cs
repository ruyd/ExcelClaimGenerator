using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ExcelClaimGenerator
{

    public static partial class TypeEx
    {
 
        public static string Clean(this string s, int? max = null)
        {
            if (s == null)
                return null;
            s = s.Trim();
            if (max.HasValue && s.Length > max)
                s = s.Substring(0, max.Value);

            return s.Trim();
        }

        public static string Inject(this string s, string prefix = null, string suffix = null)
        {
            return string.IsNullOrWhiteSpace(s) ? "" : string.Concat(prefix, ' ', s, suffix);
        }
 
    }
}

