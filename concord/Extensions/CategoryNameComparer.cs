using System.Collections.Generic;

namespace concord.Extensions
{
    internal class CategoryNameComparer : IEqualityComparer<string>
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
    }
}