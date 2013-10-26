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
        private const string LibraryName = "$lib$";

        private static ILogger _logger;
        private static IRunnerSettingsBuilder _runnerSettingsBuilder;

        private static void Main(string[] args)
        {
            _logger = ServiceLocator.Instance.Get<ILogger>();
            _runnerSettingsBuilder = ServiceLocator.Instance.Get<IRunnerSettingsBuilder>();
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
                string outputPrefix,
                int? concurrentThreads,
                string categories,
                bool rerunFailedCategories)
            {
                var serviceLocator = ServiceLocator.Instance;

                var builderFactory = serviceLocator.Get<IRunnerFactory>();

                try
                {
                    var runnerSettings = _runnerSettingsBuilder.SetOutputFolder(@out);
                    if (!string.IsNullOrEmpty(outputPrefix))
                    {
                        runnerSettings.PrependFilenames(
                            outputPrefix.Replace(
                                LibraryName,
                                Path.GetFileNameWithoutExtension(lib)));
                    }
                    var batchBuilder = builderFactory.Create(runnerSettings.Build(), lib, rerunFailedCategories, categories);

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

            [Verb]
            public static void Debug()
            {
                Console.WriteLine("Attach if you want to, then press any key");
                Console.ReadKey(true);

                Console.WriteLine("Debugging paths:");
                Console.WriteLine("Nunit: " + Settings.Instance.NunitPath);
                Console.WriteLine("Report Gen: " + Settings.Instance.NunitReportGeneratorPath);
            }
        }
    }
}