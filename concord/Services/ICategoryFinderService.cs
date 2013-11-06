using System;
using System.Collections.Generic;
using System.Reflection;

namespace concord.Services
{
    public interface ICategoryFinderService
    {
        IList<Type> FindTestFixtures(Assembly assembly, Func<Type, bool> filterTestFixtures = null);
        IEnumerable<string> FindTestFixturesExcludingCategories(Assembly assembly, IEnumerable<string> excludeCategories, Func<Type, bool> filterTestFixtures = null);
        IList<string> FindCategories(Assembly assembly, Func<Type, bool> filterTestFixtures = null);
    }
}