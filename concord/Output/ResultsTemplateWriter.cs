using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
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
            //var useful1 = typeof (JsonConvert);
            var useful2 = typeof (MvcHtmlString);
            var useful3 = typeof(HtmlString);

            var outputBody = _templateService.Parse<T>(outputTemplate, model, null, typeof (T).Name);
            //var outputBody = _templateService.Parse(outputTemplate, model, null, typeof (T).Name);

            if (outputFile != null)
                File.WriteAllText(outputFile, outputBody);
            return outputBody;
        }
    }
}