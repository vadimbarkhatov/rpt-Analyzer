using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHEORptAnalyzer
{
    public static class Extensions
    {
        public static IEnumerable<T> Gate<T>(this IEnumerable<T> left, bool gateBool)
            => gateBool ? left : Enumerable.Empty<T>();

        public static U Apply<T, U>(this T left, Func<T, U> func)
            => func(left);

        public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right)
            => a => left(a) && right(a);

        public static Func<T, bool> Or<T>(this Func<T, bool> left, Func<T, bool> right)
                => a => left(a) || right(a);

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
