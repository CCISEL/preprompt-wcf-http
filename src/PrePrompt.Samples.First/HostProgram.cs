using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Common;
using PrePROMPT.Samples.Common;

namespace PrePrompt.Samples.First
{

    public class FirstHostConfiguration : HttpHostConfiguration, IProcessorProvider
    {

        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            // No request processors
        }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            processors.ClearMediaTypeProcessors();
            //processors.Add(new JsonProcessor(operation, mode));
            /*
            processors.Add(new AtomMediaTypeProcessor(operation, mode)
                .WithFormatter(
                    (TheService.TimeZoneListModel tzms) => new SyndicationFeed("Time zones", "List of time zones", null,
                                                                               tzms.Zones.Select(
                                                                                   tzm =>
                                                                                   new SyndicationItem(tzm.Id, tzm.Name,
                                                                                                       new Uri(tzm.Uri))))));
            */
            processors.Add(new ImageFromTextMediaProcessor(operation, mode));
        }
    }

    class HostProgram
    {
        public const string ArgumentHttpRequestMessage = "HttpRequestMessage";
        public const string ArgumentHttpResponseMessage = "HttpResponseMessage";
        public const string ArgumentUri = "Uri";
        public const string ArgumentRequestHeaders = "RequestHeaders";
        public const string ArgumentResponseHeaders = "ResponseHeaders";

        static IEnumerable<ProcessorArgument> GetInputArguments()
        {
            var args = new List<ProcessorArgument>();
            args.Add(new ProcessorArgument(ArgumentHttpRequestMessage, typeof(HttpRequestMessage)));
            args.Add(new ProcessorArgument(ArgumentHttpResponseMessage, typeof(HttpResponseMessage)));
            args.Add(new ProcessorArgument(ArgumentUri, typeof(Uri)));
            args.Add(new ProcessorArgument(ArgumentRequestHeaders, typeof(HttpRequestHeaders)));
            args.Add(new ProcessorArgument(ArgumentResponseHeaders, typeof(HttpResponseHeaders)));
            return args;
        }

        static IEnumerable<ProcessorArgument> GetOutputArguments(HttpOperationDescription hod)
        {
            var args = new List<ProcessorArgument>();
            foreach (var parameter in hod.InputParameters)
            {
                string parameterName = null;

                if (parameter.ParameterType == typeof(HttpRequestMessage))
                {
                    parameterName = ArgumentHttpRequestMessage;
                }
                else if (parameter.ParameterType == typeof(HttpResponseMessage))
                {
                    parameterName = ArgumentHttpResponseMessage;
                }
                else
                {
                    parameterName = parameter.Name;
                }

                args.Add(new ProcessorArgument(parameterName, parameter.ParameterType));
            }
            return args;
        }

        static void Pipeline(HttpHostConfiguration cfg)
        {
            var baseAddress = new Uri("http://localhost:8080/first/");
            var hod = ContractDescription.GetContract(typeof (TheService)).Operations.Where(o => o.Name == "GetTime").
                First().ToHttpOperationDescription();

            var builder = new PipelineBuilder();

            var processors = new List<Processor>();
            processors.Add(new UriTemplateProcessor(baseAddress, new UriTemplate(hod.GetUriTemplateString())));
            processors.Add(new XmlProcessor(hod, MediaTypeProcessorMode.Request));
            cfg.ProcessorProvider.RegisterRequestProcessorsForOperation(hod, processors, MediaTypeProcessorMode.Request);
            var pipeline = builder.Build(processors, GetInputArguments(), GetOutputArguments(hod));
            var req = new HttpRequestMessage();
            var resp = new HttpResponseMessage();
            var inputs = new object[]
                             {
                                 req,
                                 resp,
                                 new Uri("http://localhost:8080/first/time/"),
                                 req.Headers,
                                 resp.Headers
                             };
            var outputs = pipeline.Execute(inputs);

        }

        static void Main(string[] args)
        {

            //var config = new FirstHostConfiguration();
            var prov = new ProcessorProviderFor<TheService>();
            prov.RemoveAllMediaTypeProcessors().ForResponses.OfAllOperations();

            /*prov.Use((o, l, m) => new ImageFromTextMediaProcessor(o, m))
                .ForResponses.OfOperation(
                s => s.GetTime2(default(HttpRequestMessage), default(HttpResponseMessage)));*/
            prov.Use((o, l, m) => new WaveFromTextMediaProcessor(o, m))
                .ForResponses.OfOperation(
                s => s.GetTime2(default(HttpRequestMessage), default(HttpResponseMessage)));

            prov.Use((o, l, m) => new AtomMediaTypeProcessor(o, m)
                    .WithFormatter(
                        (TheService.TimeZoneListModel tzms) => new SyndicationFeed("Time zones", "List of time zones", null,
                                                                               tzms.Zones.Select(
                                                                                   tzm =>
                                                                                   new SyndicationItem(tzm.Id, tzm.Name,
                                                                                                       new Uri(tzm.Uri)))))
                )
                .ForResponses.OfOperation(
                    s => s.GetZones(default(HttpRequestMessage), default(HttpResponseMessage)));

            //prov.Use((o,l,m) => new DataValidationProcessor(o)).ForRequests.OfAllOperations();
            //prov.Use((o,l,m) => new RequestLoggingProcessor(o)).ForRequests.OfAllOperations();

            var config = new FirstHostConfiguration().SetProcessorProvider(prov).ResourceLinks(reg =>
            {

            });

            Pipeline(config);

            using(var host = new WebHttpServiceHost(typeof(TheService)))
            {
                var ep = host.AddServiceEndpoint(typeof(TheService), new HttpMessageBinding(){TransferMode = TransferMode.Streamed}, "http://localhost:8080/first/");
                ep.Behaviors.Add(new HttpEndpointBehavior(config));
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", ep.Address);
                Console.ReadLine();
            }

        }
    }
}
