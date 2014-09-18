using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace concord.Output.Dto
{
    public interface IRunStatsCollectionVersioning
    {
        RunStatsCollection LoadRunOrder(string path);
        RunStatsCollection MigrateCollection(RunStatsCollection collection);
    }

    public class RunStatsCollectionVersioning : IRunStatsCollectionVersioning
    {
        public const int TargetVersion = 2;

        public RunStatsCollection LoadRunOrder(string path)
        {
            var collection = TryLoadKnownVersionsRunOrder(path);

            collection = MigrateCollection(collection);
            if (collection.StatsVersion != TargetVersion)
            {
                throw new Exception("WTF write your migration code better... StatsCollection is not current version");
            }

            return collection;
        }

        private RunStatsCollection TryLoadKnownVersionsRunOrder(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File must exist to call this");

            var deserializers = new List<Func<string, RunStatsCollection>>
            {
                DeserializeRunOrder,
                DeserializeRunOrder_Version0
            };

            Exception firstException = null;
            foreach (var deserializer in deserializers)
            {
                try
                {
                    var collection = deserializer(path);
                    if (collection != null)
                        return collection;
                }
                catch (Exception ex)
                {
                    if (firstException == null)
                        firstException = ex;
                }
            }
            if (firstException != null)
                throw firstException;

            throw new Exception("WTF deserialization totally failed");
        }

        public RunStatsCollection MigrateCollection(RunStatsCollection collection)
        {
            if (collection.StatsVersion == TargetVersion)
            {
                return collection;
            }

            if (collection.StatsVersion == 0)
            {
                var stats = collection.Records.ToList();
                stats.ForEach(x =>
                {
                    x.WeightedAverageTime = x.AverageTime;
                    x.FailedAverageTime = x.AverageTime;
                });
                collection.Records = stats;
                collection.StatsVersion = 2;
                return collection;
            }
            if (collection.StatsVersion == 1)
            {
                throw new NotSupportedException("There really was no version 1...");
            }
            if (collection.StatsVersion == 2)
            {
                //Current version
                return collection;
            }

            throw new IndexOutOfRangeException("Unknown StatsVersion");
        }

        private static RunStatsCollection DeserializeRunOrder(string path)
        {
            return JsonConvert.DeserializeObject<RunStatsCollection>(File.ReadAllText(path));
        }

        private static RunStatsCollection DeserializeRunOrder_Version0(string path)
        {
            var stats = JsonConvert.DeserializeObject<IEnumerable<RunStats>>(File.ReadAllText(path));
            return RunStatsCollection.BuildVersion0(stats);
        }
    }
}