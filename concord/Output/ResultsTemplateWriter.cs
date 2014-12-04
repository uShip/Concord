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
        private static string TemplateFolderPath;

        private readonly ITemplateService _templateService;

        public ResultsTemplateWriter(ITemplateService templateService)
        {
            _templateService = templateService;

            //Find location of this executable
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            TemplateFolderPath = Path.Combine(basePath, "RazorTemplates");
        }

        public string OutputResults<T>(T model, string outputFile = null)
        {
            try
            {
                if (!Directory.Exists(TemplateFolderPath))
                {
                    return "Error: Path not found";
                }
                var outputTemplate = File.ReadAllText(Path.Combine(TemplateFolderPath, typeof(T).Name + ".cshtml"));

                //Need actual references
                //var useless1 = typeof (StructureMap.Container);
                //var useful1 = typeof (JsonConvert);
                var useful2 = typeof(MvcHtmlString);
                var useful3 = typeof(HtmlString);

                var outputBody = _templateService.Parse<T>(outputTemplate, model, null, typeof(T).Name);
                //var outputBody = _templateService.Parse(outputTemplate, model, null, typeof (T).Name);

                if (outputFile != null)
                    File.WriteAllText(outputFile, outputBody);
                return outputBody;
            }
            catch (System.Exception ex)
            {
                if (outputFile != null)
                    File.WriteAllText(outputFile, ex.ToString());
                return ex.ToString();
            }
        }
    }
}