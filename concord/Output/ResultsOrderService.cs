using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using concord.Configuration;
using concord.Extensions;
using concord.Output.Dto;
using Newtonsoft.Json;

namespace concord.Output
{
    public interface IResultsOrderService
    {
        void OutputRunOrder(IEnumerable<RunStats> runners, List<string> skippedTests);
        IEnumerable<string> GetCategoriesInDesiredOrder();
    }

    public class ResultsOrderService : IResultsOrderService
    {
        private readonly IRunnerSettings _settings;

        public ResultsOrderService(IRunnerSettings settings)
        {
            _settings = settings;
        }

        public void OutputRunOrder(IEnumerable<RunStats> runners, List<string> skippedTests)
        {
            var path = _settings.ResultsOrderDataFilepath;
            if (string.IsNullOrEmpty(path)) return;

            var runOrderData = new List<RunStats>();
            var runHistoryLookup = new Dictionary<string, RunHistoryStats>();
            try
            {
                var previousData = LoadPreviousRunOrder().ToList();
                //Only copy over data from skippedTests
                //  So if any are deleted they won't remain in there
                runOrderData.AddRange(previousData.Where(x => skippedTests.Contains(x.Name)));
//                //Copy over failed tests
//                runOrderData.AddRange(runners.Where(x => !x.IsSuccess));
                //Copy over other cases...  TODO This will not work for uncategorized...
                if (!runners.Any(x => x.Name == "all"))
                {
                    var testDataForOthers = previousData.FirstOrDefault(x => x.Name == "all");
                    if (testDataForOthers != null) runOrderData.Add(testDataForOthers);
                }

                runOrderData.Each(x => --x.TestRunId);


                //Keep average stats:
                runHistoryLookup = previousData.ToDictionary(k => k.Name, v => (RunHistoryStats)v);
            }
            catch (Exception)
            {
                //Ignore this
            }

            //Add all new data
            runOrderData.AddRange(runners);
//            runOrderData.AddRange(runners.Where(x => x.IsSuccess));

            //Keep average stats:
            runOrderData.Each(x =>
            {
                if (runHistoryLookup.ContainsKey(x.Name))
                {
                    var history = runHistoryLookup[x.Name];
                    x.CopyHistoryStatsFrom(history);
                }
                if (x.IsCurrentRun)
                {
                    x.AddDatapoint(x.RunTime, x.IsSuccess);
                }
            });

            //Write data to file
            File.WriteAllText(path, JsonConvert.SerializeObject(runOrderData.OrderBy(x => x.Name))
                                               .Replace("},", "}," + Environment.NewLine));
        }

        private IEnumerable<RunStats> LoadPreviousRunOrder()
        {
            var path = _settings.ResultsOrderDataFilepath;
            return File.Exists(path)
                ? JsonConvert.DeserializeObject<IEnumerable<RunStats>>(File.ReadAllText(path)).Where(x => x != null)
                : new List<RunStats>();
        }

        private IEnumerable<string> GetCategoriesInOrder()
        {
            return LoadPreviousRunOrder()
                .OrderByDescending(x => x.WeightedAverageTime)
                .ThenByDescending(x => x.AverageTime) //For reverse compatibility only
                .ThenByDescending(x => x.RunTime) //Empty file case
                .Select(x => x.Name);
        }

        /// <summary>
        /// This should work the best, ideally would spread out some of the starting
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetCategoriesAlternated()
        {
            return GetCategoriesInOrder()
                .AlternateFromHalf();
        }

        public IEnumerable<string> GetCategoriesInDesiredOrder()
        {
            return GetCategoriesAlternated();
        }
    }
}