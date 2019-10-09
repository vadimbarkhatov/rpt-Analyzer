using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHEORptAnalyzer
{
        public static class Extensions
        {

                public static Func<IEnumerable<T>, IEnumerable<T>> Combine<T>(this Func<IEnumerable<T>, IEnumerable<T>> left, Func<IEnumerable<T>, IEnumerable<T>> right)
                {
                    return x => left(x).Concat(right(x));
                }

                public static Func<IEnumerable<T>, IEnumerable<T>> Filter<T>(this Func<IEnumerable<T>, IEnumerable<T>> left, Func<T, bool> right)
                {
                    return x => left(x).Where(right);
                }

                public static IEnumerable<T> Gate<T>(this IEnumerable<T> left, bool gateBool)
                {
                    return gateBool ? left : Enumerable.Empty<T>();
                }

                public static U X<T, U>(this T left, Func<T, U> func)
                {
                    return func(left);
                }

        public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right)
                {
                    return a => left(a) && right(a);
                }

                    public static Func<T, bool> Or<T>(this Func<T, bool> left, Func<T, bool> right)
                        => a => left(a) || right(a);
        }
}
