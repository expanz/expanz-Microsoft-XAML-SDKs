using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Expanz.Extensions.BCL
{
    public static class StringExtension
    {
        private static readonly Regex emailExpression = new Regex(@"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$", RegexOptions.Singleline);
        private static readonly Regex webUrlExpression = new Regex(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        
        [DebuggerStepThrough]
        public static string FormatWith(this string instance, params object[] args)
        {
            Check.Argument.IsNotNullOrEmpty(instance, "instance");

            return string.Format(Culture.Current, instance, args);
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string target)
        {
            return string.IsNullOrEmpty(target);
        }

        [DebuggerStepThrough]
        public static string NullSafe(this string target)
        {
            return (target ?? string.Empty).Trim();
        }

#if ! SILVERLIGHT && ! WINDOWS_PHONE
        [DebuggerStepThrough]
        public static string Hash(this string instance)
        {
            Check.Argument.IsNotNullOrEmpty(instance, "instance");
            
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = Encoding.Unicode.GetBytes(instance);
                byte[] hash = md5.ComputeHash(data);

                return Convert.ToBase64String(hash);
            }
        }
#endif


        [DebuggerStepThrough]
        public static string WrapAt(this string target, int index)
        {
            const int DotCount = 3;

            Check.Argument.IsNotEmpty(target, "target");
            Check.Argument.IsNotNegativeOrZero(index, "index");

            return (target.Length <= index) ? target : string.Concat(target.Substring(0, index - DotCount), new string('.', DotCount));
        }

        [DebuggerStepThrough]
        public static Guid ToGuid(this string target)
        {
            Guid result = Guid.Empty;

            if ((!string.IsNullOrEmpty(target)) && (target.Trim().Length == 22))
            {
                string encoded = string.Concat(target.Trim().Replace("-", "+").Replace("_", "/"), "==");

                try
                {
                    byte[] base64 = Convert.FromBase64String(encoded);

                    result = new Guid(base64);
                }
                catch (FormatException)
                {
                }
            }

            return result;
        }

        [DebuggerStepThrough]
        public static T ToEnum<T>(this string instance, T defaultValue) where T : IComparable, IFormattable
        {
            T convertedValue = defaultValue;

            if (!string.IsNullOrEmpty(instance))
            {
                try
                {
                    convertedValue = (T)Enum.Parse(typeof(T), instance.Trim(), true);
                }
                catch (ArgumentException)
                {
                }
            }

            return convertedValue;
        }

        [DebuggerStepThrough]
        public static bool IsEmail(this string instance)
        {
            return !string.IsNullOrEmpty(instance) && emailExpression.IsMatch(instance);
        }

        [DebuggerStepThrough]
        public static bool IsWebUrl(this string instance)
        {
            return IsNullOrEmpty(instance) && webUrlExpression.IsMatch(instance);
        }

        [DebuggerStepThrough]
        public static string Replace(this string target, ICollection<string> oldValues, string newValue)
        {
            oldValues.Each(oldValue => target = target.Replace(oldValue, newValue));
            return target;
        }

        //[DebuggerStepThrough]
        //public static bool IsWebUrl(this string instance)
        //{
        //    return !string.IsNullOrEmpty(instance) && WebUrlExpression.IsMatch(instance);
        //}

        //[DebuggerStepThrough]
        //public static bool IsIpAddress(this string instance)
        //{
        //    IPAddress ip;

        //    return !string.IsNullOrEmpty(instance) && IPAddress.TryParse(instance, out ip);
        //}

        //[DebuggerStepThrough]
        //public static string UrlEncode(this string target)
        //{
        //    return HttpUtility.UrlEncode(target);
        //}

        //[DebuggerStepThrough]
        //public static string UrlDecode(this string target)
        //{
        //    return HttpUtility.UrlDecode(target);
        //}

        //[DebuggerStepThrough]
        //public static string AttributeEncode(this string target)
        //{
        //    return HttpUtility.HtmlAttributeEncode(target);
        //}

        //[DebuggerStepThrough]
        //public static string HtmlEncode(this string target)
        //{
        //    return HttpUtility.HtmlEncode(target);
        //}

        //[DebuggerStepThrough]
        //public static string HtmlDecode(this string target)
        //{
        //    return HttpUtility.HtmlDecode(target);
        //}
    }
}