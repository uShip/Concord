using System;
using System.IO;
using RazorEngine.Templating;

namespace concord.Output
{
    public interface IResultsTemplateWriter
    {
        string OutputResults<T>(T model, string outputFile = null);
    }

    public class ResultsTemplateWriter : IResultsTemplateWriter
    {
        static readonly string TemplateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RazorTemplates");

        private readonly ITemplateService _templateService;

        public ResultsTemplateWriter(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        public string OutputResults<T>(T model, string outputFile = null)
        {
            var outputTemplate = File.ReadAllText(Path.Combine(TemplateFolderPath, typeof(T).Name + ".cshtml"));

            //Need actual references
            //var useless1 = typeof (StructureMap.Container);

            var outputBody = _templateService.Parse<T>(outputTemplate, model, null, typeof (T).Name);
            //var outputBody = _templateService.Parse(outputTemplate, model, null, typeof (T).Name);

            if (outputFile != null)
                File.WriteAllText(outputFile, outputBody);
            return outputBody;
        }
    }
}