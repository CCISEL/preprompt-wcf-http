using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Http;

namespace PrePrompt.Samples.Common
{
    public class PipelineHost
    {
        // Standard arguments names
        public const string ArgumentHttpRequestMessage = "HttpRequestMessage";
        public const string ArgumentHttpResponseMessage = "HttpResponseMessage";
        public const string ArgumentUri = "Uri";
        public const string ArgumentRequestHeaders = "RequestHeaders";
        public const string ArgumentResponseHeaders = "ResponseHeaders";

        private readonly Pipeline _pipeline;
        private readonly ProcessorArgument[] _args;

        static IEnumerable<ProcessorArgument> GetInputArguments()
        {
            return new List<ProcessorArgument>()
                           {
                               new ProcessorArgument(ArgumentHttpRequestMessage, typeof (HttpRequestMessage)),
                               new ProcessorArgument(ArgumentHttpResponseMessage, typeof (HttpResponseMessage)),
                               new ProcessorArgument(ArgumentUri, typeof (Uri)),
                               new ProcessorArgument(ArgumentRequestHeaders, typeof (HttpRequestHeaders)),
                               new ProcessorArgument(ArgumentResponseHeaders, typeof (HttpResponseHeaders))
                           };
        }

        static IEnumerable<ProcessorArgument> GetOutputArguments(HttpOperationDescription hod)
        {
            return hod.InputParameters.Select(p =>
                                              new ProcessorArgument(
                                                  p.ParameterType == typeof (HttpRequestMessage) ? ArgumentHttpRequestMessage
                                                      : p.ParameterType == typeof (HttpResponseMessage) ? ArgumentHttpResponseMessage
                                                      : p.Name,
                                                  p.ParameterType));
        }

        public PipelineHost(HttpHostConfiguration cfg, Type serviceType, string operationName, string baseAddress)
            : this(cfg, ContractDescription.GetContract(serviceType).Operations.Where(o => o.Name == operationName).
                            First().ToHttpOperationDescription(), baseAddress)
        {}

        public PipelineHost(HttpHostConfiguration cfg, HttpOperationDescription hod, string baseAddress)
        {
            var builder = new PipelineBuilder();

            var processors = new List<Processor>();
            processors.Add(new UriTemplateProcessor(new Uri(baseAddress), new UriTemplate(hod.GetUriTemplateString())));
            processors.Add(new XmlProcessor(hod, MediaTypeProcessorMode.Request));
            if(cfg != null)
                cfg.ProcessorProvider.RegisterRequestProcessorsForOperation(hod, processors, MediaTypeProcessorMode.Request);
            _args = GetOutputArguments(hod).ToArray();
            _pipeline = builder.Build(processors, GetInputArguments(), _args);
        }

        public PipelineExecutionResult RunPipeline(HttpRequestMessage req, HttpResponseMessage resp, Uri address)
        {
            var inputs = new object[]
                             {
                                 req,
                                 resp,
                                 address,
                                 req.Headers,
                                 resp.Headers
                             };
            var result = _pipeline.Execute(inputs);
            return new PipelineExecutionResult(_args, result.Output);
        }
    }

    public class PipelineExecutionResult
    {
        private readonly ProcessorArgument[] _args;
        private readonly object[] _outputs;

        public PipelineExecutionResult(ProcessorArgument[] args, object[] outputs)
        {
            if(args.Length != outputs.Length)
            {
                throw new InvalidOperationException("Length mismatch between output ProcessorArgument and output values");
            }
            _args = args;
            _outputs = outputs;
        }

        public T Get<T>(int i)
        {
            if(i<0 || i>= _outputs.Length) throw new InvalidOperationException("out of bound");
            if(!typeof(T).IsAssignableFrom(_args[i].ArgumentType)) throw new InvalidOperationException("Invalid type");
            return (T) _outputs[i];
        }

        public T Get<T>(string name)
        {
            var res =
                _args
                .Select((p, i) => new {Argument = p, Index = i})
                .Where(t => t.Argument.Name == name).
                    FirstOrDefault();
            if(res == null) throw new InvalidOperationException("unexistant argument");
            if(!typeof(T).IsAssignableFrom(res.Argument.ArgumentType)) throw new InvalidOperationException("Invalid type");
            return (T) _outputs[res.Index];
        }

    }
}
