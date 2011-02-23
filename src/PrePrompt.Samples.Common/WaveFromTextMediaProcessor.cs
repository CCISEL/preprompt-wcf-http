using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Speech.Synthesis;
using System.Text;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Common
{
    public class WaveFromTextMediaProcessor : MediaTypeProcessor
    {

        public WaveFromTextMediaProcessor(HttpOperationDescription operation, MediaTypeProcessorMode mode)
            :base(operation,mode)
        {
        }

        public override IEnumerable<string> SupportedMediaTypes
        {
            get { yield return "audio/x-wav"; }
        }

        public override void WriteToStream(object instance, System.IO.Stream stream, System.Net.Http.HttpRequestMessage request)
        {
            var text = instance as string;
            if (text == null) return;
            using(var synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveStream(stream);
                //synth.Rate -= 10;
                synth.Speak("current time is "+text);
            }
        }

        public override object ReadFromStream(System.IO.Stream stream, System.Net.Http.HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }
    }
}
