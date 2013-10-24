using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CLAP;
using concord.Configuration;
using concord.Factories;
using concord.Logging;
using concord.Nunit;

namespace concord
{
    internal class Program
    {
        private static ILogger _logger;

        private static void Main(string[] args)
        {
            _logger = ServiceLocator.Instance.Get<ILogger>();
            Parser.Run<CommandLineRunner>(args);
        }


        private class CommandLineRunner
        {
            [Verb]
            public static void Merge(
                string path,
                string @out)
            {
                var resultMerger = ServiceLocator.Instance.Get<IResultMerger>();
                File.WriteAllText(@out, resultMerger.MergeResults(path).XmlOutput);
            }

            [Verb]
            public static void RunReport(
                string lib,
                string @out,
                int? concurrentThreads,
                string categories,
                bool rerunFailedCategories)
            {
                var serviceLocator = ServiceLocator.Instance;

                var builderFactory = serviceLocator.Get<IRunnerFactory>();

                _logger.Log("Report at" + @out);
                try
                {
                    var outputPath = @out ?? Environment.CurrentDirectory;
                    _logger.Log("Output at" + @out);

                    var batchBuilder = builderFactory.Create(lib, rerunFailedCategories, categories, outputPath);

                    batchBuilder.GetRunResultsAsXml(concurrentThreads.HasValue ? concurrentThreads.Value : 15);
                }
                catch (ReflectionTypeLoadException loadException)
                {
                    _logger.Log("Load exception: " + loadException);
                    var loaderExceptions = loadException.LoaderExceptions.Select(x => x.ToString());
                    var message = string.Join("\n", loaderExceptions);
                    Console.WriteLine("{0}\n{1}", loadException, message);
                }
                catch (Exception exception)
                {
                    _logger.Log("Exception: " + exception);
                    Console.WriteLine(exception.ToString());
                }
            }
        }
    }
}