using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Http;

namespace PrePrompt.Samples.Async
{
    public class FirstHostConfiguration : HttpHostConfiguration, IProcessorProvider
    {

        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                          MediaTypeProcessorMode mode)
        {
            // No request processors
        }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                           MediaTypeProcessorMode mode)
        {
            processors.ClearMediaTypeProcessors();
            prepareAsyncOperation(operation, processors);
            processors.Add(new JsonProcessor(operation, mode));
        }

        private static void prepareAsyncOperation(HttpOperationDescription operation, IList<Processor> processors)
        {
            var retValue = operation.ReturnValue;
            if (typeof(Task).IsAssignableFrom(retValue.ParameterType))
            {
                operation.Behaviors.Add(new OperationIsAsync());
                retValue.ParameterType = ReflectionHelper.GetFutureResultType(retValue.ParameterType) ?? typeof(void);
                //processors.Add(new AsyncProcessor(retValue));
            }
        }
    }

    class HostProgram
    {
        static void Main()
        {
            using (var host = new WebHttpServiceHost(typeof(TheService)))
            {
                var ep = host.AddServiceEndpoint(typeof(TheService), new HttpMessageBinding(), "Http://localhost:8080/async/");
                ep.Behaviors.Add(new AsyncHttpEndpointBehavior(new FirstHostConfiguration()));
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", ep.Address);
                Console.ReadLine();
            }
        }
    }
}