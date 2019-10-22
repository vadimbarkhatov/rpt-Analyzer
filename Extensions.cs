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

        public static IEnumerable<T> ConcatSingle<T>(this IEnumerable<T> enumerable, T value)
        {
            return enumerable.Concat(new[] { value });
        }

        public static U GetTupleValue<T, U>(this IEnumerable<(T, U)> left, T key)
            => left.Where(x => x.Item1.Equals(key)).First().Item2;

        public static void CrystalSyntaxHighlight(FastColoredTextBox textBox)
        {
            TextStyle StringStyle = new TextStyle(System.Drawing.Brushes.Black, null, System.Drawing.FontStyle.Regular);
            TextStyle FuncStyle = new TextStyle(System.Drawing.Brushes.Blue, null, System.Drawing.FontStyle.Regular);
            TextStyle TypeStyle = new TextStyle(System.Drawing.Brushes.DarkRed, null, System.Drawing.FontStyle.Regular);
            TextStyle CommentStyle = new TextStyle(System.Drawing.Brushes.Green, null, System.Drawing.FontStyle.Regular);
            TextStyle FieldStyle = new TextStyle(System.Drawing.Brushes.Black, null, System.Drawing.FontStyle.Regular);



            textBox.AddStyle(CommentStyle);
            textBox.AddStyle(FieldStyle);
            textBox.AddStyle(StringStyle);
            textBox.AddStyle(TypeStyle);
            textBox.AddStyle(FuncStyle);

            string crystalFuncs =
                "(in|and|or|if|then|else|like|not|for|LeftOuter|Equal|AlertMessage|IsAlertTriggered|IsAlertEnabled|Average|Count|DistinctCount|MakeArray|Maximum|Minimum|PopulationStdDev|PopulationVariance|StdDev|Sum|UBound|Variance|CurrentDate|CurrentDateTime|CurrentTime|Date|DateAdd|DateDiff|DatePart|DateSerial|DateTime|DateTimeValue|DateValue|Day|DayOfWeek|Friday|Monday|Saturday|Sunday|Thursday|Tuesday|UseSystem|Wednesday|FirstFourDays|FirstFullWeek|FirstJan1|UseSystem|Hour|IsDate|IsDateTime|IsTime|Minute|Month|MonthName|Second|ShiftDateTime|Time|Timer|TimeSerial|TimeValue|WeekdayName|Year|Aged0To30Days|Aged31To60Days|Aged61To90Days|AllDatesFromToday|AllDatesFromTomorrow|AllDatesToToday|AllDatesToYesterday|Calendar1stHalf|Calendar1stQtr|Calendar2ndHalf|Calendar2ndQtr|Calendar3rdQtr|Calendar4thQtr|Last4WeeksToSun|Last7Days|LastFullMonth|LastFullWeek|LastYearMTD|LastYearYTD|MonthToDate|Next30Days|Next31To60Days|Next61To90Days|Next91To365Days|Over90Days|WeekToDateFromSun|YearToDate|ContentLocale|CurrentCEUserID|CurrentCEUserName|CurrentCEUserTimeZone|DataDate|DataTime|DataTimeZone|FileAuthor|FileCreationDate|Filename|GroupingLevel|ModificationDate|ModificationTime|PrintDate|PrintTime|PrintTimeZone|ReportComments|ReportTitle|SelectionLocale|BeforeReadingRecords|EvaluateAfter|WhilePrintingRecords|WhileReadingRecords|Abs|Atn|Ceiling|Cos|Exp|Floor|Int|Log|MRound|crPi|Remainder|Rnd|Round|RoundUp|Sgn|Sin|Sqr|Tan|Truncate|GetValueDescriptions|CountHierarchicalChildren|CurrentPageOrientation|DrillDownGroupLevel|GroupNumber|GroupSelection|HasValue|HierarchyLevel|InRepeatedGroupHeader|IsNull|Next|NextIsNull|OnFirstRecord|OnLastRecord|PageNofM|PageNumber|Previous|PreviousIsNull|RecordNumber|RecordSelection|TotalPageCount|Choose|IIF|Switch|GetLowerBound|GetUpperBound|HasLowerBound|HasUpperBound|IncludesLowerBound|IncludesUpperBound|AscW|ChrW|Filter|InStr|InStrRev|Join|Left|Length|LowerCase|Mid|NumericText|ProperCase|Replace|ReplicateString|Right|Roman|Space|Split|StrCmp|StrReverse|ToNumber|ToText|ToWords|Trim|TrimLeft|TrimRight|UpperCase|URLDecode|URLEncode|Val|Average|Correlation|Count|Covariance|DistinctCount|Maximum|Median |Minimum|Mode|NthLargest|NthMostFrequent|NthSmallest|PercentOfAverage|PercentOfDistinctCount|PercentOfMinimum|PercentOfSum|PopulationVariance|PopulationStdDev|PthPercentile|StdDev|Sum|Variance|WeightedAverage)"; //Crystal Functions
            //string crystalTypes = "(String|Number|Unknown|Date|DateTime|Boolean)"; //Crystal Functions

            textBox.Range.SetStyle(CommentStyle, @"(\/\/).*", RegexOptions.IgnoreCase);
            textBox.Range.SetStyle(StringStyle, @"(""(.*?)""|'(.*?)')", RegexOptions.IgnoreCase);
            textBox.Range.SetStyle(TypeStyle, @"(?<=} : )(.*)", RegexOptions.IgnoreCase);
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

    public class IsGreaterOrEqualThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IComparable v = value as IComparable;
            IComparable p = parameter as IComparable;

            if (v == null || p == null)
                throw new FormatException("to use this converter, value and parameter shall inherit from IComparable");

            return (v.CompareTo(p) >= 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
