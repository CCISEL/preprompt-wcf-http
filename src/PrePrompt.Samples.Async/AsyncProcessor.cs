using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Async
{
    public class AsyncProcessor : Processor
    {
        private readonly HttpParameterDescription _retValue;
        private readonly Type _futureResultType;

        public AsyncProcessor(HttpOperationDescription operation, IEnumerable<Processor> processors)
        {
            _retValue = operation.ReturnValue;
            _futureResultType = ReflectionHelper.GetFutureResultType(_retValue.ParameterType);

            //
            // Does not work because of the Parameter property in MediaTypeProcessor.
            //

            processors
                .OfType<MediaTypeProcessor>()
                .SelectMany(p => p.InArguments)
                .Where(arg => arg.Name == operation.ReturnValue.Name)
                .ForEach(arg => arg.ArgumentType = _futureResultType);
        }

        protected override ProcessorResult OnExecute(object[] input)
        {
            Console.WriteLine("tururu");
            //return new ProcessorResult { Output = new[] { ReflectionHelper.GetFutureResultGetMethod(input[0].GetType()).Invoke(input[0], new object[0])} };
            return new ProcessorResult { Output = input };
        }

        protected override IEnumerable<ProcessorArgument> OnGetInArguments()
        {
            return new[] { new ProcessorArgument(_retValue.Name, _retValue.ParameterType) };
        }

        protected override IEnumerable<ProcessorArgument> OnGetOutArguments()
        {
            return new[] { new ProcessorArgument(_retValue.Name, _futureResultType ?? _retValue.ParameterType) };
        }
    }
}