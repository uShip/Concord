using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using concord.Configuration;
using concord.Extensions;

namespace concord.Services
{
    internal class CategoryFinderService : ICategoryFinderService
    {
        public IList<Type> FindTestFixtures(Assembly assembly, Func<Type, bool> filterTestFixtures = null)
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

            if (filterTestFixtures != null)
                types = types.Where(filterTestFixtures);

            return types
                .Where(isTest)
                .ToArray();
        }

        public IEnumerable<string> FindTestFixturesExcludingCategories(Assembly assembly, IEnumerable<string> excludeCategories, Func<Type, bool> filterTestFixtures = null)
        {
            var testFixtures = FindTestFixtures(assembly, filterTestFixtures);
            return testFixtures.Where(x => !x.HasCategoryAttribute(excludeCategories))
                               .Where(t => !RunnerSettingsSingleton.Instance.IncludeIgnoredFeaturesInStats && t.IsIgnored())
                               .Select(x => x.FullName);
        }

        public IList<string> FindCategories(Assembly assembly, Func<Type, bool> filterTestFixtures = null)
        {
            var fixtures = FindTestFixtures(assembly, filterTestFixtures);
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