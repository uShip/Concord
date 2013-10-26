using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using concord.Extensions;

namespace concord.Services
{
    internal class CategoryFinderService : ICategoryFinderService
    {
        public IList<Type> FindTestFixtures(Assembly assembly)
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
            return types
                .Where(isTest)
                .ToArray();
        }

        public IEnumerable<string> FindTestFixturesExcludingCategories(Assembly assembly, IEnumerable<string> excludeCategories)
        {
            var testFixtures = FindTestFixtures(assembly);
            return testFixtures.Where(x => !x.HasCategoryAttribute(excludeCategories))
                               .Select(x => x.AssemblyQualifiedName);
        }

        public IList<string> FindCategories(Assembly assembly)
        {
            var fixtures = FindTestFixtures(assembly);
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