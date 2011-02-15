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
        public override IEnumerable<string> SupportedMediaTypes
        {
            get { yield return "application/atom+xml"; }
        }
        public AtomMediaTypeProcessor(HttpOperationDescription operation, MediaTypeProcessorMode mode): base(operation, mode)
        {
        }

        public override void WriteToStream(object instance, System.IO.Stream stream, HttpRequestMessage request)
        {
            var courses = instance as IEnumerable<Course>;
            if (courses == null)
            {
                // return a 500?
                return;
            }
            var feed = new SyndicationFeed(
                courses.Select(c => new SyndicationItem(c.Name, c.Syllabus, c.HtmlSyllabus)));
            using(var writer = XmlWriter.Create(stream))
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
