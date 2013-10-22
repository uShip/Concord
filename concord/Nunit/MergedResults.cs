using System.Collections.Generic;
using System.IO;

namespace concord.Nunit
{
    public class MergedResults
    {
        public string XmlOutput { get; set; }

        public List<FailedCategory> FailedCategories { get; set; }

        public int Total { get; set; }
        public int Errors { get; set; }
        public int Failures { get; set; }
//        public int Notrun { get; set; }
//        public int Inconclusive { get; set; }
        public int Ignored { get; set; }
//        public int Skipped { get; set; }
//        public int Invalid { get; set; }
    }

    public class FailedCategory
    {
        public string FileName { get; set; }

        public string Name
        {
            get { return Path.GetFileNameWithoutExtension(FileName); }
        }
    }
}