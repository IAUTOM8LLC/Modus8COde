using IAutoM8.InfusionSoft.Interfaces;
using IAutoM8.InfusionSoft.Responces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IAutoM8.InfusionSoft
{
    abstract class BaseService
    {
        private readonly IInfusionSoftConfiguration _infusionSoftConfiguration;
        private readonly ILogger _logger;
        protected BaseService(IInfusionSoftConfiguration infusionSoftConfiguration,
            ILogger logger)
        {
            _infusionSoftConfiguration = infusionSoftConfiguration;
            _logger = logger;
        }

        protected string GenerateDoc(Action<XmlWriter> docBodyGenerator)
        {
            var stringBuilder = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            }))
            {
                xmlWriter.WriteStartElement("methodCall");
                docBodyGenerator(xmlWriter);
                xmlWriter.WriteEndElement();
            }
            return stringBuilder.ToString();
        }

        protected async Task<string> PostAndGetResponceAsync(string doc)
        {
            var client = new HttpClient();
            var content = new StringContent(doc, Encoding.UTF8, "text/xml");
            _logger.LogInformation(doc);
            var response = await client.PostAsync(_infusionSoftConfiguration.GetApiUri(), content);
            var responceContent= await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responceContent);
            return responceContent;
        }
    }
}
