using System;
using System.Collections.Generic;
using System.Reflection;

namespace concord.Services
{
    public interface ICategoryFinderService
    {
        IList<Type> FindTestFixtures(Assembly assembly);
        IList<string> FindCategories(Assembly assembly);
    }
}