using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace concord.Configuration
{
    internal class Settings : ISettings
    {
        private static readonly Lazy<ISettings> LazyInstance = new Lazy<ISettings>(() => new Settings());
        private readonly string _assemblyPath;
        private readonly string _toolPackagesPath;
        private readonly string _libPath;

        private Settings()
        {
            _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_assemblyPath == null)
                throw new InvalidOperationException("The assembly provided is not a valid nunit test runner");
            _toolPackagesPath = Path.Combine(_assemblyPath, @"..\..\..\packages");

            //_libPath = FindValidPath(_assemblyPath, new[]
            //    {
            //        @"lib", //Nuget location
            //        @"..\..\lib", //From bin
            //        @"..\lib" //why not
            //    });
            //if (_libPath == null) throw new FileNotFoundException("Could not find the lib folder!");
        }

        private static string FindValidPath(string basePath, IEnumerable<string> possiblePaths)
        {
            return possiblePaths.Select(x =>
                {
                    var tryPath = Path.Combine(basePath, x);
                    return Directory.Exists(tryPath)
                               ? tryPath
                               : null;
                }).FirstOrDefault();
        }

        /// <summary>
        /// Find the dependency folder, without depending on the version number
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startsWith"></param>
        /// <returns></returns>
        private static string FindMatchingDirectory(string path, string startsWith)
        {
            string searchPattern = startsWith + "*";
            return Directory.GetDirectories(path, searchPattern)
                            .OrderByDescending(x => x)
                            .FirstOrDefault();
        }

        private string BuildDependencyPath(string dependencyName, string filepath)
        {
            var dependencyDirectory = FindMatchingDirectory(_toolPackagesPath, dependencyName);
            return Path.Combine(dependencyDirectory, filepath);
        }


        public static ISettings Instance
        {
            get { return LazyInstance.Value; }
        }

        public string NunitPath
        {
            get { return BuildDependencyPath(@"NUnit.Runners", @"tools\nunit-console.exe"); }
        }

        public string NunitReportGeneratorPath
        {
            get { return BuildDependencyPath(@"NUnit2Report.Console.Runner", @"NUnit2Report.Console.exe"); }
        }
    }
}