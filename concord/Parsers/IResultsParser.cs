using System.Collections.Generic;

namespace concord.Parsers
{
    public interface IResultsParser
    {
        IEnumerable<string> GetErrorsCategories(string fileName);
        IEnumerable<string> GetErrorsCategories(IEnumerable<string> fileLines);
    }
}