using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Http;

namespace PrePrompt.Samples.Common
{
    public class WebLinkingProcessorProvider : IProcessorProvider
    {
        private readonly IProcessorProvider _inner;

        public WebLinkingProcessorProvider(WebLinksRegistry config, IProcessorProvider inner)
        {
            LinksRegistry = config;
            _inner = inner;
        }

        public WebLinksRegistry LinksRegistry { get; private set; }

        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                          MediaTypeProcessorMode mode)
        {
            _inner.RegisterRequestProcessorsForOperation(operation, processors, mode);
            processors.Add(new WebLinkingProcessor(LinksRegistry, operation, mode));
        }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                           MediaTypeProcessorMode mode)
        {
            _inner.RegisterResponseProcessorsForOperation(operation, processors, mode);
            processors.Add(new WebLinkingProcessor(LinksRegistry, operation, mode));
        }
    }

    internal class WebLinkingProcessor : Processor
    {
        private readonly WebLinksRegistry _registry;
        private readonly OperationDescription _operation;
        private readonly MediaTypeProcessorMode _mode;

        public WebLinkingProcessor(WebLinksRegistry registry, HttpOperationDescription httpOperationDescription,
                                   MediaTypeProcessorMode mode)
        {
            _registry = registry;
            _operation = httpOperationDescription.ToOperationDescription();
            _mode = mode;
        }

        protected override ProcessorResult OnExecute(object[] input)
        {
            if (_mode == MediaTypeProcessorMode.Request)
            {
                return new ProcessorResult { Output = new object[] { _registry.GetLinksFor(_operation) } };
            }

            var httpResponse = (HttpResponseMessage)input[0];
            //var links = (WebLinkCollection)input[1];
            var links = _registry.GetLinksFor(_operation);
            
            httpResponse.Headers.AddWithoutValidation("Link", "lalal");

            return new ProcessorResult();
        }

        protected override IEnumerable<ProcessorArgument> OnGetInArguments()
        {
            if (_mode == MediaTypeProcessorMode.Request)
            {
                return null;
            }

            return new[] 
            { 
                new ProcessorArgument(HttpPipelineFormatter.ArgumentHttpResponseMessage, typeof(HttpResponseMessage)),
                //new ProcessorArgument("webLinks", typeof(WebLinkCollection))
            };
        }

        protected override IEnumerable<ProcessorArgument> OnGetOutArguments()
        {
            if (_mode == MediaTypeProcessorMode.Request)
            {
                return new[] { new ProcessorArgument("webLinks", typeof(WebLinkCollection)) };
            }

            return null;
        }
    }
}