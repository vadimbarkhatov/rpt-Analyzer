using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHEORptAnalyzer
{
        public static class Extensions
        {
            public static Func<T, IEnumerable<T>> Combine<T>(this Func<T, IEnumerable<T>> left, Func<T, IEnumerable<T>> right)
            {
                return x => left(x).Concat(right(x));
            }

            public static Func<T, IEnumerable<T>> Filter<T>(this Func<T, IEnumerable<T>> left, Func<T, bool> right)
            {
                return x => left(x).Where(right);
            }

        public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right)
            {
                return a => left(a) && right(a);
            }

            public static Func<T, bool> Or<T>(this Func<T, bool> left, Func<T, bool> right)
                => a => left(a) || right(a);
        }
}
