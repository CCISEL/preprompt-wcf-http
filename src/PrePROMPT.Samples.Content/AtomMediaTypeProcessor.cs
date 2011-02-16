using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Description;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Microsoft.ServiceModel.Http;
using Prompt.Data;

namespace PrePROMPT.Samples.Content
{

    class AtomMediaTypeProcessor : MediaTypeProcessor
    {

        private readonly Dictionary<Type, Func<object, SyndicationFeed>> _formatters = new Dictionary<Type, Func<object, SyndicationFeed>>();

        public AtomMediaTypeProcessor WithFormatter<T>(Func<T, SyndicationFeed> f)
        {
            _formatters.Add(typeof(T), o => f((T)o));
            return this;
        }

        public override IEnumerable<string> SupportedMediaTypes
        {
            get { yield return "application/atom+xml"; }
        }
        public AtomMediaTypeProcessor(HttpOperationDescription operation, MediaTypeProcessorMode mode): base(operation, mode)
        {
        }

        public override void WriteToStream(object instance, System.IO.Stream stream, HttpRequestMessage request)
        {
            var instanceType = instance.GetType();
            Func<object, SyndicationFeed> f = _formatters.Where(e => e.Key.IsAssignableFrom(instanceType)).FirstOrDefault().Value;
            if(f == null)
            {
                // what to do? return a 500?
                return;
            }
            var feed = f(instance);
            using(var writer = XmlWriter.Create(stream, new XmlWriterSettings{CloseOutput = false}))
            {
                new Atom10FeedFormatter(feed).WriteTo(writer);    
            }
        }

        public override object ReadFromStream(System.IO.Stream stream, HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }
    }
}
