using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace concord.Nunit
{
    internal class ResultMerger : IResultMerger
    {
        public MergedResults MergeResults(string path)
        {
            var files = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly);
            return MergeFiles(files);
        }

        public static MergedResults MergeFiles(IEnumerable<string> files)
        {
            XElement environment = null;
            XElement culture = null;
            var suites = new List<XElement>();

            bool finalSuccess = true;
            string finalResult = "";
            double totalTime = 0;
            int total = 0, errors = 0, failures = 0, notrun = 0, inconclusive = 0, ignored = 0, skipped = 0, invalid = 0;
            foreach (var tr in files.Select(XDocument.Load).Select(doc => doc.Element("test-results")))
            {
                if (environment == null)
                    environment = tr.Element("environment");
                if (culture == null)
                    culture = tr.Element("culture-info");

                total += Convert.ToInt32(tr.Attribute("total").Value);
                errors += Convert.ToInt32(tr.Attribute("errors").Value);
                failures += Convert.ToInt32(tr.Attribute("failures").Value);
                notrun += Convert.ToInt32(tr.Attribute("not-run").Value);
                inconclusive += Convert.ToInt32(tr.Attribute("inconclusive").Value);
                ignored += Convert.ToInt32(tr.Attribute("ignored").Value);
                skipped += Convert.ToInt32(tr.Attribute("skipped").Value);
                invalid += Convert.ToInt32(tr.Attribute("invalid").Value);

                var ts = tr.Element("test-suite");
                if (ts == null) return null;
                var result = ts.Attribute("result").Value;

                if (!Convert.ToBoolean(ts.Attribute("success").Value))
                    finalSuccess = false;

                totalTime += Convert.ToDouble(ts.Attribute("time").Value);

                if (finalResult != "Failure" && (String.IsNullOrEmpty(finalResult) || result == "Failure" || finalResult == "Success"))
                    finalResult = result;

                suites.Add(ts);
            }

            if (String.IsNullOrEmpty(finalResult))
            {
                finalSuccess = false;
                finalResult = "Inconclusive";
            }

            var project = XElement.Parse(String.Format("<test-suite type=\"Test Project\" name=\"\" executed=\"True\" result=\"{0}\" success=\"{1}\" time=\"{2}\" asserts=\"0\" />",
                                                       finalResult, finalSuccess ? "True" : "False", totalTime));
            var results = XElement.Parse("<results/>");
            results.Add(suites.ToArray());
            project.Add(results);

            var now = DateTime.Now;
            var trfinal = XElement.Parse(String.Format("<test-results name=\"Merged results\" total=\"{0}\" errors=\"{1}\" failures=\"{2}\" not-run=\"{3}\" inconclusive=\"{4}\" ignored=\"{5}\" skipped=\"{6}\" invalid=\"{7}\" date=\"{8}\" time=\"{9}\" />",
                                                       total, errors, failures, notrun, inconclusive, ignored, skipped, invalid, now.ToString("yyyy-MM-dd"), now.ToString("HH:mm:ss")));
            trfinal.Add(new[] { environment, culture, project });
            var textWriter = new StringWriter();
            XmlWriter xmlWriter = new XmlTextWriter(textWriter);
            trfinal.WriteTo(xmlWriter);
            //return textWriter.GetStringBuilder().ToString();

            return new MergedResults
                {
                    XmlOutput = textWriter.GetStringBuilder().ToString(),
                    Total = total,
                    Errors = errors,
                    Failures = failures,
                    Ignored = ignored
                };
        }
    }
}