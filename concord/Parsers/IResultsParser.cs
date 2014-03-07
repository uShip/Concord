using System.Collections.Generic;

namespace concord.Parsers
{
    public interface IResultsParser
    {
        //IEnumerable<string> ReadFileLines(string fileName);
        IEnumerable<string> GetErrorsCategories(string fileName);
        //IEnumerable<string> GetCategoriesInOrder(string fileName);

        IEnumerable<string> GetErrorsCategories(IEnumerable<string> fileLines);
        //IEnumerable<string> GetCategoriesInOrder(IEnumerable<string> fileLines);
    }
}