using System;
using System.Collections.Generic;
using System.Linq;
using concord.Configuration;

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

        public static bool IsIgnored(this Type t)
        {
            return t.GetCustomAttributesEndingWith("IgnoreAttribute").Any();
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
            return a != null
                ? a.GetType().GetProperty("Name").GetValue(a).ToString()
                : null;
        }

        private static string GetCategoryAttribute(Type t)
        {
            if (!RunnerSettingsSingleton.Instance.IncludeIgnoredFeaturesInStats
                && t.IsIgnored()) return null;

            //TODO this is failing... give priority to ones without underscores, or with fewer numbers?
            var attributes = t.GetCustomAttributesEndingWith("CategoryAttribute").ToArray()
                              .OrderTicketNumbersLower();
            var firstAttribute = attributes.FirstOrDefault();

            if (firstAttribute == null) return null;

            var firstAttributeName = GetCategoryName(firstAttribute);
            return firstAttributeName;
        }

        private static IEnumerable<Attribute> OrderTicketNumbersLower(this IEnumerable<Attribute> attributes)
        {
            return attributes.OrderBy(GetCategoryName, CategoryNameComparer.Default);
        }

        public static bool HasCategoryAttribute(this Type t, IEnumerable<string> categories)
        {
            var categoryAttribute = GetCategoryAttribute(t);
            return categoryAttribute != null
                   && categories.Contains(categoryAttribute, CategoryNameComparer.Default);
        }

    }
}