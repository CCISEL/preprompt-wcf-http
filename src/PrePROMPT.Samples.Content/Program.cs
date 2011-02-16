using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Text;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Http;
using Prompt.Data;

namespace PrePROMPT.Samples.Content
{
    public class FirstHostConfiguration : HttpHostConfiguration, IProcessorProvider
    {

        public void RegisterRequestProcessorsForOperation(System.ServiceModel.Description.HttpOperationDescription operation, IList<System.ServiceModel.Dispatcher.Processor> processors, MediaTypeProcessorMode mode)
        {
            // No request processors
        }

        public void RegisterResponseProcessorsForOperation(System.ServiceModel.Description.HttpOperationDescription operation, IList<System.ServiceModel.Dispatcher.Processor> processors, MediaTypeProcessorMode mode)
        {
            processors.Add(new AtomMediaTypeProcessor(operation,mode)
                .WithFormatter((IEnumerable<Course> cs) =>
                                   new SyndicationFeed("courses", "courses list", new Uri("Http://localhost:8080/prompt/courses"),
                                       cs.Select( c=> new SyndicationItem(c.Name,c.Syllabus,c.HtmlSyllabus))))
                );
            processors.Add(new XmlProcessor(operation, mode));
            processors.Add(new JsonProcessor(operation,mode));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new WebHttpServiceHost(new PromptService(new InMemoryPromptRepository())))
            {
                var ep = host.AddServiceEndpoint(typeof(PromptService), new HttpMessageBinding(), "Http://localhost:8080/prompt/");
                ep.Behaviors.Add(new HttpEndpointBehavior(new FirstHostConfiguration()));
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", ep.Address);
                Console.ReadLine();
            }
        }
    }
}
