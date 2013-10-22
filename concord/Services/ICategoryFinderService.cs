using System.Collections.Generic;
using System.Reflection;

namespace concord.Services
{
    public interface ICategoryFinderService
    {
        IList<string> FindCategories(Assembly assembly);
    }
}