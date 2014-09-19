using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace concord.Extensions
{
    internal class CategoryNameComparer : IEqualityComparer<string>, IComparer<string>
    {
        public static readonly CategoryNameComparer Default = new CategoryNameComparer();

        public bool Equals(string x, string y)
        {
            return x.ToLowerInvariant() == y.ToLowerInvariant();
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant()
                      .GetHashCode();
        }

        public int Compare(string x, string y)
        {
            //If one has more digits or underscores than the other, that is lower
            var countX = new Regex(@"[\d_]").Matches(x).Count;
            var countY = new Regex(@"[\d_]").Matches(y).Count;

            return countX != countY
                ? countX.CompareTo(countY)
                : string.CompareOrdinal(x, y);
        }
    }
}