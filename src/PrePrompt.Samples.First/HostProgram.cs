﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Syndication;
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
        

        static void Main(string[] args)
        {

            //var config = new FirstHostConfiguration();
            var prov = new ProcessorProviderFor<TheService>();
            prov.RemoveAll().OnResponses.OfOperation(
                s => s.GetTime2(default(HttpRequestMessage), default(HttpResponseMessage)));
            prov.Use((o, l, m) => new ImageFromTextMediaProcessor(o, m)).OnResponses.OfOperation(
                    s => s.GetTime2(default(HttpRequestMessage), default(HttpResponseMessage)));

            var config = new FirstHostConfiguration().SetProcessorProvider(prov);

            using(var host = new WebHttpServiceHost(typeof(TheService)))
            {
                var ep = host.AddServiceEndpoint(typeof(TheService), new HttpMessageBinding(), "http://localhost:8080/first/");
                ep.Behaviors.Add(new HttpEndpointBehavior(config));
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", ep.Address);
                Console.ReadLine();
            }

        }
    }
}