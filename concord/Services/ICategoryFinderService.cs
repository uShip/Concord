using System;
using System.Collections.Generic;
using System.Reflection;

namespace concord.Services
{
    public interface ICategoryFinderService
    {
        IList<Type> FindTestFixtures(Assembly assembly);
        IEnumerable<string> FindTestFixturesExcludingCategories(Assembly assembly, IEnumerable<string> excludeCategories);
        IList<string> FindCategories(Assembly assembly);
    }
}