using System.Collections.Generic;

namespace concord.Extensions
{
    internal class CategoryNameComparer : IEqualityComparer<string>
    {
        public static readonly CategoryNameComparer Default = new CategoryNameComparer();

        public bool Equals(string x, string y)
        {
            return x.TrimLongPrefix().ToLowerInvariant() == y.TrimLongPrefix().ToLowerInvariant();
        }

        public int GetHashCode(string obj)
        {
            return obj.TrimLongPrefix()
                      .ToLowerInvariant()
                      .GetHashCode();
        }
    }
}