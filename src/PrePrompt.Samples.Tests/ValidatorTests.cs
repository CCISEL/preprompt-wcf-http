using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Http;
using NUnit.Framework;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Tests
{

    [ServiceContract]
    interface ServiceClass
    {
        [WebGet(UriTemplate = "/{str}")]
        void Oper(
            DataValidationResult isValid,
            [StringLength(4)] string str);
    }

    class ProcessorProvider : IProcessorProvider
    {
        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            processors.Add(new DataValidationProcessor(operation));
        }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            throw new NotImplementedException();
        }
    }

    [TestFixture]
    class ValidatorTests
    {
        [Test]
        public void ValidationModuleValidatesParameterStringAnnotations()
        {
            var config = new HttpHostConfiguration().SetProcessorProvider(new ProcessorProvider());
            var host = new PipelineHost(config, typeof (ServiceClass), "Oper", "http://host.com/");

            var uri = new Uri("http://host.com/333");
            var res = host.RunPipeline(new HttpRequestMessage(), new HttpResponseMessage(), uri);
            Assert.True(res.Get<DataValidationResult>(0).Valid);
            Assert.AreEqual("333", res.Get<string>(1));

            uri = new Uri("http://host.com/55555");
            res = host.RunPipeline(new HttpRequestMessage(), new HttpResponseMessage(), uri);
            Assert.AreEqual("55555", res.Get<string>(1));
        }
    }
}
