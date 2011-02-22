using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace PrePrompt.Samples.Common
{
    public class RequestLoggingProcessor : Processor
    {
        private readonly HttpOperationDescription _oper;
        private readonly ProcessorArgument[] _inputArgs;

        public RequestLoggingProcessor(HttpOperationDescription oper)
        {
            Trace.AutoFlush = true;
            _oper = oper;
            _inputArgs =  _oper.InputParameters
                .Where(p => p.ParameterType != typeof(HttpRequestMessage)
                    && p.ParameterType != typeof(HttpResponseMessage))
                .Select(p => new ProcessorArgument(p.Name, p.ParameterType)).ToArray();
        }

        protected override IEnumerable<ProcessorArgument> OnGetInArguments()
        {
            return _inputArgs;
        }

        protected override IEnumerable<ProcessorArgument> OnGetOutArguments()
        {
            yield break;
        }

        protected override ProcessorResult OnExecute(object[] input)
        {
            Console.WriteLine("--Begin request log--");
            int i = 0;
            foreach(object obj in input)
            {
                Console.WriteLine("{0}: {1}", _inputArgs[i++].Name, obj);
            }
            Console.WriteLine("--End request log--");
            return new ProcessorResult();
        }
    }
}
