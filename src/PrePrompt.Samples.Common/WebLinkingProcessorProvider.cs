using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
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
            WebLinkCollection linkCollection;
            var httpRequest = (HttpRequestMessage)input[0];

            if (_mode == MediaTypeProcessorMode.Request)
            {
                linkCollection = _registry.GetLinksFor(_operation);
                httpRequest.GetProperties().Add(linkCollection);
                return new ProcessorResult { Output = new object[] { linkCollection } };
            }
            
            var httpResponse = (HttpResponseMessage)input[1];
            linkCollection = (WebLinkCollection)httpRequest.GetProperties().Single(p => p is WebLinkCollection);

            var links = linkCollection
                .Links
                .Aggregate(new StringBuilder(), (b, target) => b.Append(extractLinkDescription(target)).Append(','))
                .RemoveLastCharacter()
                .ToString();

            if (links.IsNullOrEmpty() == false)
            {
                httpResponse.Headers.AddWithoutValidation("Link", links);
            }

            return new ProcessorResult();
        }

        private static string extractLinkDescription(WebLinkTarget target)
        {
            //
            // TODO: Add support for properties.
            //
               
            return "{0};rel=\"{1}\"".FormatWith(target.Uri, target.RelationType);
        }

        protected override IEnumerable<ProcessorArgument> OnGetInArguments()
        {
            yield return new ProcessorArgument(HttpPipelineFormatter.ArgumentHttpRequestMessage, typeof(HttpRequestMessage));
            if (_mode == MediaTypeProcessorMode.Response)
            {
                yield return new ProcessorArgument(HttpPipelineFormatter.ArgumentHttpResponseMessage, typeof(HttpResponseMessage));
            }
        }

        protected override IEnumerable<ProcessorArgument> OnGetOutArguments()
        {
            if (_mode == MediaTypeProcessorMode.Request)
            {
                yield return new ProcessorArgument("webLinks", typeof(WebLinkCollection));
            }
        }
    }
}