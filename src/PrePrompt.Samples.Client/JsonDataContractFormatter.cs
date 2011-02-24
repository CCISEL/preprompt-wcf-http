using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using Microsoft.Net.Http;

namespace PrePrompt.Samples.Client
{
    public class JsonDataContractFormatter : IContentFormatter
    {
        private readonly DataContractJsonSerializer _serializer;

        public JsonDataContractFormatter(DataContractJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public IEnumerable<MediaTypeHeaderValue> SupportedMediaTypes
        {
            get { return new[] { new MediaTypeHeaderValue("application/json") { CharSet = "utf-8"} }; }
        }

        public void WriteToStream(object instance, Stream stream)
        {
            _serializer.WriteObject(stream, instance);
        }

        public object ReadFromStream(Stream stream)
        {
            return _serializer.ReadObject(stream);
        }
    }
}