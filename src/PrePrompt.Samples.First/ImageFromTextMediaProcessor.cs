using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.First
{
    class ImageFromTextMediaProcessor : MediaTypeProcessor
    {

        public ImageFromTextMediaProcessor(HttpOperationDescription operation, MediaTypeProcessorMode mode)
            :base(operation,mode)
        {
        }

        public override IEnumerable<string> SupportedMediaTypes
        {
            get { yield return "image/jpeg"; }
        }

        public override void WriteToStream(object instance, System.IO.Stream stream, System.Net.Http.HttpRequestMessage request)
        {
            var text = instance as string;
            if (text == null) return;
            ImageTools.WriteJpegCreatedFrom(text, stream);
        }

        public override object ReadFromStream(System.IO.Stream stream, System.Net.Http.HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }
    }
}
