using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CHEORptAnalyzer
{
    public static class Extensions
    {
        public static IEnumerable<T> Gate<T>(this IEnumerable<T> left, bool gateBool)
            => gateBool ? left : Enumerable.Empty<T>();

        public static U Apply<T, U>(this T left, Func<T, U> func)
            => func(left);

        public static string Combine(this IEnumerable<string> left, string seperator)
            => string.Join(seperator, left);

        public static string AppendToNewLine(this string left, string text)
            => text + left.Split(new string[] { "\n"}, StringSplitOptions.None).Combine(Environment.NewLine + text);

        public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right)
            => a => left(a) && right(a);

        public static Func<T, bool> Or<T>(this Func<T, bool> left, Func<T, bool> right)
            => a => left(a) || right(a);

        public static IEnumerable<T> GetEnumValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static U GetTupleValue<T, U>(this IEnumerable<(T, U)> left, T key)
            => left.Where(x => x.Item1.Equals(key)).First().Item2;

        public static void CrystalSyntaxHighlight(FastColoredTextBox textBox)
        {
            TextStyle StringStyle = new TextStyle(System.Drawing.Brushes.Black, null, System.Drawing.FontStyle.Regular);
            TextStyle FuncStyle = new TextStyle(System.Drawing.Brushes.Blue, null, System.Drawing.FontStyle.Regular);
            TextStyle CommentStyle = new TextStyle(System.Drawing.Brushes.Green, null, System.Drawing.FontStyle.Regular);
            TextStyle FieldStyle = new TextStyle(System.Drawing.Brushes.Black, null, System.Drawing.FontStyle.Regular);

            textBox.AddStyle(CommentStyle);
            textBox.AddStyle(FieldStyle);
            textBox.AddStyle(StringStyle);
            textBox.AddStyle(FuncStyle);

            string crystalFuncs = "(in|and|or|if|then|else|like|not)";

            textBox.Range.SetStyle(CommentStyle, @"(\/\/).*", RegexOptions.IgnoreCase);
            textBox.Range.SetStyle(StringStyle, @"""(.*?)""", RegexOptions.IgnoreCase);
            textBox.Range.SetStyle(FuncStyle, crystalFuncs, RegexOptions.IgnoreCase);
            textBox.Range.SetStyle(FieldStyle, @"{(.*?)}", RegexOptions.IgnoreCase);
        }

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

        [DllImport("mpr.dll")]
        static extern int WNetGetUniversalNameA(string lpLocalPath, int dwInfoLevel, IntPtr lpBuffer, ref int lpBufferSize);

        public static string ToLiteDBID(string path)
        {
            return "$/" + path
                        .Replace("\\\\", string.Empty)
                        .Replace("\\", "/")
                        .Replace(":", string.Empty)
                        .Replace(" ", "_")
                        .Replace("(", "@")
                        .Replace("&", ";")
                        .Replace(",", "%")
                        .Replace(")", "!");
        }

        // I think max length for UNC is actually 32,767
        public static string LocalToUNC(string localPath, int maxLen = 2000)
        {
            IntPtr lpBuff;

            // Allocate the memory
            try
            {
                lpBuff = Marshal.AllocHGlobal(maxLen);
            }
            catch (OutOfMemoryException)
            {
                return null;
            }

            try
            {
                int res = WNetGetUniversalNameA(localPath, 1, lpBuff, ref maxLen);

                if (res != 0)
                    return null;

                // lpbuff is a structure, whose first element is a pointer to the UNC name (just going to be lpBuff + sizeof(int))
                return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(lpBuff));
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(lpBuff);
            }
        }
    }

    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}
