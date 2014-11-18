using System;
using System.Collections.Generic;
using concord.Output.Dto;

namespace concord.RazorTemplates.Models
{
    public class FancyResults
    {
        public TimeSpan TotalRuntime { get; set; }
        public IEnumerable<RunStats> Runners { get; set; }
        public List<string> SkippedTests { get; set; }
    }
}