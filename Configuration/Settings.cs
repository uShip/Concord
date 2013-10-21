using System;
using System.IO;
using System.Reflection;

namespace concord.Configuration
{
    internal class Settings : ISettings
    {
        private static readonly Lazy<ISettings> LazyInstance = new Lazy<ISettings>(() => new Settings());
        private readonly string _assemblyPath;
        private readonly string _packagesPath;

        private Settings()
        {
            _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_assemblyPath == null)
                throw new InvalidOperationException("The assembly provided is not a valid nunit test runner");
            _packagesPath = Path.Combine(_assemblyPath, @"..\..\packages");
        }

        public static ISettings Instance
        {
            get { return LazyInstance.Value; }
        }

        public string NunitPath
        {
            get { return Path.Combine(_packagesPath, @"NUnit.Runners.2.6.2\tools\nunit-console.exe"); }
        }

        public string NunitReportPath
        {
            get { return Path.Combine(_packagesPath, @"NUnit2Report.Console.Runner.1.0.0.0\NUnit2Report.Console.exe"); }
        }
    }
}