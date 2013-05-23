using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class Extensions
    {
        public static string AsBase64(this string Value)
        {
            if (string.IsNullOrEmpty(Value))
                return null;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Value));
        }

        public static string FromBase64(this string Value)
        {
            if (string.IsNullOrEmpty(Value))
                return null;
            return Encoding.UTF8.GetString(Convert.FromBase64String(Value));
        }

        public static string Substitute(this string Value, IDictionary<string, string> Variables)
        {
            StringBuilder retVal = new StringBuilder(Value);
            foreach (var v in Variables)
            {
                retVal.Replace(String.Format("%{0}%", v.Key), v.Value);
            }
            return retVal.ToString();
        }

        public static IDictionary<string, string> ParseNameValue(this string Value)
        {
            IDictionary<string, string> retVal = new Dictionary<string, string>();
            var start = Value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in start)
            {
                var result = s.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (result.Length > 1)
                    retVal.Add(result[0], result[1]);
            }
            return retVal;
        }

    }
}
