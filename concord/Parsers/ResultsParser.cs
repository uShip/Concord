using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using concord.Extensions;

namespace concord.Parsers
{
    internal class ResultsParser : IResultsParser
    {
        public IEnumerable<string> ReadFileLines(string fileName)
        {
            return !File.Exists(fileName)
                       ? new string[0]
                       : File.ReadLines(fileName);
        }

        public IEnumerable<string> GetErrorsCategories(string fileName)
        {
            return GetErrorsCategories(ReadFileLines(fileName));
        }

        public IEnumerable<string> GetCategoriesInOrder(string fileName)
        {
            return GetCategoriesInOrder(ReadFileLines(fileName));
        }

        public IEnumerable<string> GetErrorsCategories(IEnumerable<string> fileLines)
        {
            return ParseFileLines(fileLines)
                .Where(x => x.ExitCode > 0)
                .Select(x => x.FeatureName);
        }

        public IEnumerable<string> GetCategoriesInOrder(IEnumerable<string> fileLines)
        {
            return ParseFileLines(fileLines)
                .OrderByDescending(x => x.Runtime)
                .Select(x => x.FeatureName);
        }

        public IEnumerable<PreviousResultData> ParseFileLines(IEnumerable<string> fileLines)
        {
            var regex = new Regex(@"^(?<runtime>[\d:.]+) = (?<startOrder>\d+)-(?<finishOrder>\d+) -- (?<feature>.+)  ExitCode:(?<exitCode>.+)$");
            return fileLines
                .Select(line => regex.Match(line))
                .Where(x => x.Success) // && x.Groups["exitcode"].Value
                .Select(x => new PreviousResultData
                    {
                        Runtime = TimeSpan.Parse(x.Groups["runtime"].Value),
                        StartOrder = Int32.Parse(x.Groups["startOrder"].Value),
                        FinishOrder = Int32.Parse(x.Groups["finishOrder"].Value),
                        FeatureName = x.Groups["feature"].Value.TrimLongPrefix(),
                        ExitCode = Int32.Parse(x.Groups["exitCode"].Value)
                    });
        }
    }

    public class PreviousResultData
    {
        public TimeSpan Runtime { get; set; }
        public int StartOrder { get; set; }
        public int FinishOrder { get; set; }
        public string FeatureName { get; set; }
        public int ExitCode { get; set; }
    }
}