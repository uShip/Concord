using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using concord.Extensions;

namespace concord.Services
{
    internal class CategoryFinderService : ICategoryFinderService
    {
        public IList<string> FindCategories(Assembly assembly)
        {
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException loadException)
            {
                types = loadException.Types;
                if (!types.Any())
                    throw;
            }
            Func<Type, bool> isTest = t =>
                {
                    try
                    {
                        return t.GetCustomAttributesEndingWith("TestFixtureAttribute").Any();
                    }
                    catch
                    {
                        return false;
                    }
                };
            var fixtures = types
                .Where(isTest)
                .ToArray();
            return FindCategories(fixtures);
        }

        public IList<string> FindCategories(IEnumerable<Type> featureTypes)
        {
            //return featureTypes
            //    .Select(x => x.GetCustomAttributesEndingWith("CategoryAttribute").FirstOrDefault())
            //    .Where(x => x != null)
            //    .Select(x => x.GetType().GetProperty("Name").GetValue(x).ToString())
            //    .Distinct()
            //    .ToList();
            return featureTypes.GetCategories().ToList();
        }
    }
}