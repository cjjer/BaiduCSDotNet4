using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CloudAPI
{
    public static class Utilities
    {
        public static string ToQueryString(NameValueCollection nvc)
        {
            return  string.Join("&", Array.ConvertAll(nvc.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));
        }
        private static Regex _MoreBadUrl = new Regex(@"/{2,}", RegexOptions.Compiled);
        public static string KeepFineUrl(this string url)
        {
            return _MoreBadUrl.Replace(url, "/");
        }
        // from stackoverflow http://stackoverflow.com/questions/8063004/what-is-the-net-equivalent-of-the-php-function-hash-hmac
        public static string Hash(string message, byte[] secretKey, bool raw_output = false)
        {
            byte[] msgBytes = System.Text.Encoding.UTF8.GetBytes(message);
            byte[] hashBytes;
            using (HMACSHA1 hmac = new HMACSHA1(secretKey))
            {
                hashBytes = hmac.ComputeHash(msgBytes);
            }
            if (!raw_output)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)

                    sb.Append(hashBytes[i].ToString("x2"));
                hashBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            }
            return HttpUtility.UrlEncode(System.Convert.ToBase64String(hashBytes));
        }


        /// <summary>
        /// 大小写有效附加RequestQuery值
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dic"></param>
        public static Uri AddQueryString(Uri url, Dictionary<string, string> dic)
        {
            if (dic == null || dic.Count < 1) return url;
            System.Collections.Specialized.NameValueCollection qscoll = HttpUtility.ParseQueryString(url.Query);
            foreach (string key in dic.Keys)
            {
                if (!qscoll.AllKeys.Contains(key))
                {
                    qscoll.Add(key, dic[key]);
                }
            }

            return new UriBuilder(url.Scheme, url.Host, url.Port, url.AbsolutePath, ToQueryString(qscoll)).Uri;
        }

    }
}
