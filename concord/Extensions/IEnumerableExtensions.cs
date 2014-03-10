using System.Collections.Generic;
using System.Linq;

namespace concord.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> AlternateFromHalf<T>(this IEnumerable<T> input)
        {
            var half = input.Count() / 2;

            return input.Take(half).Alternate(input.Skip(half));
        }

        public static IEnumerable<T> Alternate<T>(this IEnumerable<T> input, IEnumerable<T> alternative)
        {
            using (var iterator = input.GetEnumerator())
            using (var iteratorAlt = alternative.GetEnumerator())
            {
                var done = false;
                while (!done)
                {
                    if (iterator.MoveNext())
                    {
                        yield return iterator.Current;
                    }
                    else done = true;
                    if (iteratorAlt.MoveNext())
                    {
                        yield return iteratorAlt.Current;
                        done = false;
                    }
                }
            }
        }
    }
}