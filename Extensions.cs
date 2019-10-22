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
               @"\bCountHierarchicalChildren\b|\bCurrentPageOrientation\b|\bPercentOfDistinctCount\b|\bCurrentCEUserTimeZone\b|\bInRepeatedGroupHeader\b|\bAllDatesFromTomorrow\b|\bBeforeReadingRecords\b|\bWhilePrintingRecords\b|\bGetValueDescriptions\b|\bAllDatesToYesterday\b|\bWhileReadingRecords\b|\bDrillDownGroupLevel\b|\bPopulationVariance\b|\bIncludesLowerBound\b|\bIncludesUpperBound\b|\bPopulationVariance\b|\bAllDatesFromToday\b|\bWeekToDateFromSun\b|\bCurrentCEUserName\b|\bIsAlertTriggered\b|\bPopulationStdDev\b|\bFileCreationDate\b|\bModificationDate\b|\bModificationTime\b|\bPercentOfAverage\b|\bPercentOfMinimum\b|\bPopulationStdDev\b|\bCurrentDateTime\b|\bAllDatesToToday\b|\bCalendar1stHalf\b|\bCalendar2ndHalf\b|\bLast4WeeksToSun\b|\bNext91To365Days\b|\bCurrentCEUserID\b|\bSelectionLocale\b|\bRecordSelection\b|\bReplicateString\b|\bNthMostFrequent\b|\bWeightedAverage\b|\bIsAlertEnabled\b|\bAged31To60Days\b|\bAged61To90Days\b|\bCalendar1stQtr\b|\bCalendar2ndQtr\b|\bCalendar3rdQtr\b|\bCalendar4thQtr\b|\bNext31To60Days\b|\bNext61To90Days\b|\bReportComments\b|\bGroupSelection\b|\bHierarchyLevel\b|\bPreviousIsNull\b|\bTotalPageCount\b|\bDistinctCount\b|\bDateTimeValue\b|\bFirstFourDays\b|\bFirstFullWeek\b|\bShiftDateTime\b|\bAged0To30Days\b|\bLastFullMonth\b|\bContentLocale\b|\bGroupingLevel\b|\bPrintTimeZone\b|\bEvaluateAfter\b|\bOnFirstRecord\b|\bGetLowerBound\b|\bGetUpperBound\b|\bHasLowerBound\b|\bHasUpperBound\b|\bDistinctCount\b|\bPthPercentile\b|\bAlertMessage\b|\bLastFullWeek\b|\bDataTimeZone\b|\bOnLastRecord\b|\bRecordNumber\b|\bPercentOfSum\b|\bCurrentDate\b|\bCurrentTime\b|\bWeekdayName\b|\bLastYearMTD\b|\bLastYearYTD\b|\bMonthToDate\b|\bReportTitle\b|\bGroupNumber\b|\bNumericText\b|\bCorrelation\b|\bNthSmallest\b|\bDateSerial\b|\bIsDateTime\b|\bTimeSerial\b|\bNext30Days\b|\bOver90Days\b|\bYearToDate\b|\bFileAuthor\b|\bNextIsNull\b|\bPageNumber\b|\bProperCase\b|\bStrReverse\b|\bCovariance\b|\bNthLargest\b|\bLeftOuter\b|\bMakeArray\b|\bDateValue\b|\bDayOfWeek\b|\bUseSystem\b|\bWednesday\b|\bFirstJan1\b|\bUseSystem\b|\bMonthName\b|\bTimeValue\b|\bLast7Days\b|\bPrintDate\b|\bPrintTime\b|\bRemainder\b|\bLowerCase\b|\bTrimRight\b|\bUpperCase\b|\bURLDecode\b|\bURLEncode\b|\bVariance\b|\bDateDiff\b|\bDatePart\b|\bDateTime\b|\bSaturday\b|\bThursday\b|\bDataDate\b|\bDataTime\b|\bFilename\b|\bTruncate\b|\bHasValue\b|\bPageNofM\b|\bPrevious\b|\bInStrRev\b|\bToNumber\b|\bTrimLeft\b|\bVariance\b|\bAverage\b|\bMaximum\b|\bMinimum\b|\bDateAdd\b|\bTuesday\b|\bCeiling\b|\bRoundUp\b|\bReplace\b|\bToWords\b|\bAverage\b|\bMaximum\b|\bMedian \b|\bMinimum\b|\bStdDev\b|\bUBound\b|\bFriday\b|\bMonday\b|\bSunday\b|\bIsDate\b|\bIsTime\b|\bMinute\b|\bSecond\b|\bMRound\b|\bIsNull\b|\bChoose\b|\bSwitch\b|\bFilter\b|\bLength\b|\bStrCmp\b|\bToText\b|\bStdDev\b|\bEqual\b|\bCount\b|\bMonth\b|\bTimer\b|\bFloor\b|\bRound\b|\bInStr\b|\bRight\b|\bRoman\b|\bSpace\b|\bSplit\b|\bCount\b|\bthen\b|\belse\b|\blike\b|\bDate\b|\bHour\b|\bTime\b|\bYear\b|\bcrPi\b|\bNext\b|\bAscW\b|\bChrW\b|\bJoin\b|\bLeft\b|\bTrim\b|\bMode\b|\band\b|\bnot\b|\bfor\b|\bSum\b|\bDay\b|\bAbs\b|\bAtn\b|\bCos\b|\bExp\b|\bInt\b|\bLog\b|\bRnd\b|\bSgn\b|\bSin\b|\bSqr\b|\bTan\b|\bIIF\b|\bMid\b|\bVal\b|\bSum\b|\bin\b|\bor\b|\bif\b|\bmod\b|\bdo\b|\bwhile\b|\bexit\b|\bfor\b|\bbooleanVar\b|\bcurrencyVar\b|\bdateTimeVar\b|\bdateVar\b|\bnumberVar\b|\bstringVar\b|\btimeVar\b|\bGlobal\b|\bLocal\b|\bShared\b|\bstartswith\b|\bto\b";
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
