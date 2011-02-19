using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Http;

namespace PrePrompt.Samples.First
{

    public class FirstHostConfiguration : HttpHostConfiguration, IProcessorProvider
    {

        public void RegisterRequestProcessorsForOperation(System.ServiceModel.Description.HttpOperationDescription operation, IList<System.ServiceModel.Dispatcher.Processor> processors, MediaTypeProcessorMode mode)
        {
            // No request processors
        }

        public void RegisterResponseProcessorsForOperation(System.ServiceModel.Description.HttpOperationDescription operation, IList<System.ServiceModel.Dispatcher.Processor> processors, MediaTypeProcessorMode mode)
        {
            processors.Add(new XmlProcessor(operation, mode));
            processors.Add(new JsonProcessor(operation, mode));
        }
    }

    class HostProgram
    {
        static void Main(string[] args)
        {
            using(var host = new WebHttpServiceHost(typeof(TheService)))
            {
                var ep = host.AddServiceEndpoint(typeof(TheService), new HttpMessageBinding(), "Http://localhost:8080/first/");
                ep.Behaviors.Add(new HttpEndpointBehavior(new FirstHostConfiguration()));
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", ep.Address);
                Console.ReadLine();
            }
        }
    }
}
