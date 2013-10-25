using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace concord.Parsers
{
    class ResultsParser : IResultsParser
    {
        public IEnumerable<string> GetErrorsCategories(string fileName)
        {
            return !File.Exists(fileName)
                       ? new string[0]
                       : GetErrorsCategories(File.ReadAllLines(fileName));
        }

        public IEnumerable<string> GetErrorsCategories(IEnumerable<string> fileLines)
        {
            var regex = new Regex(@"^(?<runtime>[\d:.]+) = (\d+) -- (?<feature>.+)  ExitCode:(?<exitcode>[^-0].+)$");
            return fileLines
                .Select(line => regex.Match(line))
                .Where(x => x.Success) // && x.Groups["exitcode"].Value
                .Select(x => x.Groups["feature"].Value.TrimStart('_'));
        }
    }
}