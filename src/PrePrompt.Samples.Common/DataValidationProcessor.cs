using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace PrePrompt.Samples.Common
{
    public class DataValidationResult
    {
        public bool Valid { get; internal set; }
    }

    public class DataValidationProcessor : Processor
    {
        private readonly HttpOperationDescription _oper;
        private readonly HttpParameterDescription[] _inputArgs;
        public DataValidationProcessor(HttpOperationDescription oper)
        {
            _oper = oper;
            _inputArgs = _oper.InputParameters
                .Where(p => p.ParameterType != typeof (HttpRequestMessage)
                            && p.ParameterType != typeof (HttpResponseMessage)
                            && p.ParameterType != typeof (DataValidationResult))
                .ToArray();
        }

        protected override IEnumerable<ProcessorArgument> OnGetInArguments()
        {
            return _inputArgs.Select(p => new ProcessorArgument(p.Name,p.ParameterType));
        }

        protected override IEnumerable<ProcessorArgument> OnGetOutArguments()
        {
            yield return new ProcessorArgument("isValid", typeof(DataValidationResult));
        }

        protected override ProcessorResult OnExecute(object[] input)
        {
            var mi = _oper.SyncMethod; // todo use the sync or the async
            var prms = mi.GetParameters();
            for(int i=0 ; i<_inputArgs.Length ; ++i)
            {
                foreach(var attr in prms[_inputArgs[i].Index].GetCustomAttributes(false))
                {
                    var va = attr as ValidationAttribute;
                    if (va == null) continue;
                    if(!va.IsValid(input[i]))
                    {
                        return new ProcessorResult() { Output = new object[] { new DataValidationResult { Valid = false } } };
                    }
                    
                }
            }
            return new ProcessorResult() { Output = new object[] { new DataValidationResult{Valid = true} } };
        }
    }
}
