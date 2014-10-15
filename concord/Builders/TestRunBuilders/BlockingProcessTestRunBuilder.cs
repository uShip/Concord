using System;
using System.Diagnostics;
using System.IO;
using concord.Configuration;
using NUnit.Core;

namespace concord.Builders.TestRunBuilders
{
    public class BlockingProcessTestRunBuilder : ITestRunBuilder
    {
        private string _assemblyLocation;
        private IRunnerSettings _runnerSettings;

        public void Configure(
            string assemblyLocation,
            IRunnerSettings runnerSettings)
        {
            _assemblyLocation = assemblyLocation;
            _runnerSettings = runnerSettings;
        }

        public int BuildFilteredBlockingProcess(string category, ITestFilter filter)
        {
            var args = BuildParameterString(BuildOutputXmlPath(category), ToParameterString(filter));
            return BuildBlockingProcess(args);
        }

        public int BuildFilteredBlockingProcess(string testFixture)
        {
            var args = BuildParameterString(BuildOutputXmlPath("fixture-" + testFixture), SpecifyFixture(testFixture));
            return BuildBlockingProcess(args);
        }

        private int BuildBlockingProcess(string args)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo(Settings.Instance.NunitPath, args)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                };
                var testRun = new Process
                {
                    StartInfo = processStartInfo,
                };

                //Console.WriteLine("Running: {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);
                testRun.Start();
                testRun.PriorityClass = ProcessPriorityClass.BelowNormal;
                testRun.WaitForExit();
                return testRun.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.ToString());
                Console.Error.WriteLine("nunit path: " + Settings.Instance.NunitPath);
                throw;
            }
        }

        private string BuildParameterString(string outputXmlPath, string parameters)
        {
            var namespaceFilter = "";
            if (_runnerSettings.Namespace != null)
            {
                namespaceFilter = "/run:" + _runnerSettings.Namespace;
            }
            return string.Format(@"{0} {1} /xml:{2} {3}",
                                 parameters,
                                 namespaceFilter,
                                 outputXmlPath,
                                 _assemblyLocation);
        }

        private string BuildOutputXmlPath(string category)
        {
            return Path.Combine(_runnerSettings.OutputBasePath, string.Format("{0}.xml", category));
        }

        private string ToParameterString(ITestFilter filter)
        {
            var str = filter.ToString();
            if (str.StartsWith("not "))
            {
                return "/exclude:" + str.Substring("not ".Length);
            }
            return "/include:" + str;
        }

        /// <summary>
        /// Can be a class or namespace
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        private string SpecifyFixture(string fixture)
        {
            return "/fixture:" + fixture;
        }
    }
}