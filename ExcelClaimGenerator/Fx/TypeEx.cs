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

        public static TEnum GetEnumFromString<TEnum>(string value) where TEnum : struct
        {
            TEnum result;
            if (Enum.TryParse<TEnum>(value, out result))
                return result;
            else
                return default(TEnum);
        }

        public static bool IsHtml(this string s)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(s, "<(.|\n)*?>");
        }

        public static T Deserialize<T>(this string jsonData) where T : class
        {
            if (string.IsNullOrWhiteSpace(jsonData)) return null;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonData);
        }

        public static string Serialize<T>(this T obj) where T : class
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
        public static string toString(this DateTime? date, string format = "d")
        {
            return date?.ToString(format);
        }
        public static string toString(this DateTime date, string format = "d")
        {
            return date.ToString(format);
        }
        public static string toString(this decimal? num, string format = "n1")
        {
            return num?.ToString(format);
        }
        public static string toString(this int? num, string format = "n1")
        {
            return num?.ToString(format);
        }
        public static string toString(this double? num, string format = "n1")
        {
            return num?.ToString(format);
        }
        public static string toMonthName(this DateTime? date)
        {
            //CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(8)
            return date?.ToString("MMM");
        }

        /////////////////////////////////////////////////

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

        public static string ToInitials(this string str)
        {
            return Regex.Replace(str, @"^(?'b'\w)\w*,\s*(?'a'\w)\w*$|^(?'a'\w)\w*\s*(?'b'\w)\w*$", "${a}${b}", RegexOptions.Singleline);
        }

        public static string ToCamelCase(this string s, bool force = false)
        {
            if (s == null)
                return null;

            s = s.Trim();

            //All Caps
            if (s == s.ToUpper() || force)
                s = s.ToLower();

            s = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);

            return s;
        }

        public static string StripNonAlpha(this string str, char? exclude = null)
        {
            if (String.IsNullOrWhiteSpace(str))
                return str;
            else
            {
                var s = str.Trim();
                char[] arr = s.Where(c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || exclude == c)).ToArray();
                s = new string(arr);
                return s;
            }
        }
        public static string StripNonNumeric(this string str, char? exclude = null)
        {
            if (String.IsNullOrWhiteSpace(str))
                return str;
            else
            {
                var s = str.Trim();
                char[] arr = s.Where(c => (char.IsDigit(c) || exclude == c)).ToArray();
                s = new string(arr);
                return s;
            }
        }
        public static string ToPhoneFormat(this string str, char? exclude = null)
        {
            if (String.IsNullOrWhiteSpace(str))
                return str;
            else
            {
                var clean = str.StripNonNumeric();

                return string.Format("({0}) {1}-{2}",
                    clean.Substring(0, 3),
                    clean.Substring(3, 3),
                    clean.Substring(6));
            }
        }

        internal static string Clean(object senderID)
        {
            throw new NotImplementedException();
        }

        public static string ToSafeSqlLiteral(this string s, bool doUpper = true)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            if (doUpper)
                s = s.ToUpper();

            s = s.Replace("'", "''");
            s = s.Replace("[", "[[]");
            s = s.Replace("%", "[%]");
            s = s.Replace("_", "[_]");
            s = s.Replace("execute", "BLOCKED");
            s = s.Replace("exec", "BLOCKED");
            s = s.Replace("EXECUTE", "BLOCKED");
            s = s.Replace("EXEC", "BLOCKED");
            s = s.Replace("xp_", "BLOCKED");
            s = s.Replace("?", "BLOCKED");
            s = s.Replace("--", "COMMENT");
            s = s.Replace("/*", "COMMENT");
            s = s.Replace("*/", "COMMENT");

            return s;
        }

        public static string Sql(this string s, bool writeNull = true)
        {
            return !string.IsNullOrWhiteSpace(s)
                ? $"'{s}'"
                : (writeNull ? "NULL" : null);
        }

        public static string Sql(this Guid? s, bool textNull = true)
        {
            if (s == null)
                return textNull ? "NULL" : null;
            return $"'{s}'";
        }
        public static string EmptySql(this Guid s, bool textNull = true)
        {
            if (s == Guid.Empty)
                return textNull ? "NULL" : null;
            return $"'{s}'";
        }
        public static string Sql(this DateTime? s, bool textNull = true)
        {
            if (s == null)
                return textNull ? "NULL" : null;
            return $"'{s}'";
        }
        public static string Sql(this bool? s, bool textNull = true)
        {
            if (s == null)
                return textNull ? "NULL" : null;
            return $"'{s}'";
        }
        public static string Sql(this int? s, bool textNull = true)
        {
            if (s == null)
                return textNull ? "NULL" : null;
            return $"'{s}'";
        }
        public static Guid NullEmpty(this Guid? s)
        {
            return s ?? Guid.Empty;
        }

        public static Guid ToParseAsGuid(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return Guid.Empty;

            Guid result;
            return Guid.TryParse(s, out result) ? result : Guid.Empty;
        }

        public static async Task<string> PostObjectAsync(object toSend, string url)
        {
            var webClient = new WebClient();

            var values = new NameValueCollection();

            var properties = toSend.GetType().GetProperties().ToList();
            foreach (System.Reflection.PropertyInfo p in properties)
            {
                var value = p.GetValue(toSend);
                values.Add(p.Name, value != null ? value.ToString() : null);
            }
            try
            {
                var byteResult = await webClient.UploadValuesTaskAsync(url, "POST", values);
                var result = Encoding.UTF8.GetString(byteResult);
                return result;
            }
            catch (WebException ex)
            {
                var text = new System.IO.StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static HttpClient httpClient = new HttpClient();
        public static async Task PostJsonAsync(object toSend, string url)
        {                       
            try
            {
                var response = await httpClient.PostAsync(url, new StringContent(toSend.Serialize(), Encoding.UTF8, "application/json"));
                //var content = await response.Content.ReadAsStringAsync();                
            }
            catch (WebException ex)
            {
                //var text = new System.IO.StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {

            }
        }
    }
}

