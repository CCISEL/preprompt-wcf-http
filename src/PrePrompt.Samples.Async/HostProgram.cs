using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Async
{
    public class FirstHostConfiguration : HttpHostConfiguration, IProcessorProvider
    {

        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                          MediaTypeProcessorMode mode)
        { }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                           MediaTypeProcessorMode mode)
        {
            operation.EnableAsync();
        }
    }

    class HostProgram
    {
        static void Main()
        {
            using (var host = new WebHttpServiceHost(typeof(TheService)))
            {
                var ep = host.AddServiceEndpoint(typeof(TheService), new HttpMessageBinding(), "Http://localhost:8080/async/");
                ep.Behaviors.Add(new HttpEndpointBehavior(new FirstHostConfiguration()));
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", ep.Address);
                Console.ReadLine();
            }
        }
    }
}