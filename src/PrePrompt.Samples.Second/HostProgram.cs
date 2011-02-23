using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Syndication;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Common;
using PrePROMPT.Samples.Common;

namespace PrePrompt.Samples.Second
{

    class HostProgram
    {   
        static void Main(string[] args)
        {
            var provider = new ProcessorProviderFor<TheService>();
            provider.RemoveAllMediaTypeProcessors().ForResponses.OfAllOperations();

            
            provider.Use((o, l, m) => new ImageFromTextMediaProcessor(o, m))
                .ForResponses.OfOperation(
                    s => s.GetTimeWithConneg(default(HttpRequestMessage), default(HttpResponseMessage)));

            provider.Use((o, l, m) => new WaveFromTextMediaProcessor(o, m))
                .ForResponses.OfOperation(
                    s => s.GetTimeWithConneg(default(HttpRequestMessage), default(HttpResponseMessage)));

            provider.Use((o, l, m) => 
                    new AtomMediaTypeProcessor(o, m)
                    .WithFormatter(
                        (TheService.TimeZoneListModel tzms) => new SyndicationFeed("Time zones", "List of time zones", null,
                                                                               tzms.Zones.Select(
                                                                                   tzm =>
                                                                                   new SyndicationItem(tzm.Id, tzm.Name,
                                                                                                       new Uri(tzm.Uri)))))
                )
                .ForResponses.OfOperation(
                    s => s.GetZones(default(HttpRequestMessage), default(HttpResponseMessage)));

            provider.Use((o,l,m) => new DataValidationProcessor(o)).ForRequests.OfAllOperations();
            provider.Use((o,l,m) => new RequestLoggingProcessor(o)).ForRequests.OfAllOperations();

            var config = new HttpHostConfiguration().SetProcessorProvider(provider);
            
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
