using System;
using System.Collections.Generic;
using System.Linq;

namespace concord.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Attribute> GetCustomAttributesEndingWith(this Type type, string endsWith)
        {
            return type.GetCustomAttributes(true)
                       .Where(x => x.GetType().Name.EndsWith(endsWith))
                       .Cast<Attribute>();
        }

        public static IEnumerable<string> GetCategories(this IEnumerable<Type> types)
        {
//             return types
//                 .Select(x => x.GetCustomAttributesEndingWith("CategoryAttribute").FirstOrDefault())
//                .Where(x => x != null)
//                .Select(x => x.GetType().GetProperty("Name").GetValue(x).ToString())
//                .Distinct()
//                .ToList();
            return types.Select(GetCategoryAttribute)
                        .Where(x => x != null)
                        .OrderBy(x => x)
                        .Where(x => !"long".Equals(x, StringComparison.OrdinalIgnoreCase))
                        .Distinct();
        }

        private static string GetCategoryName(Attribute a)
        {
            return a.GetType().GetProperty("Name").GetValue(a).ToString();
        }

        private static string GetCategoryAttribute(Type t)
        {
            Func<Attribute, bool> isTheLongRunningAttribute = x => "Long".Equals(GetCategoryName(x),
                                                                                 StringComparison
                                                                                     .OrdinalIgnoreCase);
            var attributes = t.GetCustomAttributesEndingWith("CategoryAttribute").ToArray();
            var firstAttribute = attributes.FirstOrDefault(x => !isTheLongRunningAttribute(x));

            if (firstAttribute == null) return null;

            var firstAttributeName = GetCategoryName(firstAttribute);
            return attributes.Any(isTheLongRunningAttribute)
                       ? "_" + firstAttributeName
                       : firstAttributeName;
        }

        public static bool HasCategoryAttribute(this Type t, IEnumerable<string> categories)
        {
            return categories.Contains(GetCategoryAttribute(t).TrimLongPrefix());
        }

        public static string TrimLongPrefix(this string category)
        {
            return category.TrimStart('_');
        }
    }
}