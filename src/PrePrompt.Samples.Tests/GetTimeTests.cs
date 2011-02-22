using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using NUnit.Framework;
using PrePrompt.Samples.First;

namespace PrePrompt.Samples.Tests
{
    [TestFixture]
    class GetTimeTests
    {
        
        [Test]
        public void GetTimeReturnsOkStatusCode()
        {
            var service = new TheService();
            var req = new HttpRequestMessage();
            var resp = new HttpResponseMessage();
            service.GetTime(req,resp);
            Assert.AreEqual(HttpStatusCode.OK,resp.StatusCode);
        }

        [Test]
        public void GetTimeReturnsCurrentTime()
        {
            var service = new TheService();
            var req = new HttpRequestMessage();
            var resp = new HttpResponseMessage();
            service.GetTime(req, resp);
            var s = resp.Content.ReadAsString();
            DateTime dt;
            Assert.True(DateTime.TryParse(s, out dt));
            var ts = DateTime.Now.Subtract(dt);
            Assert.True(ts.Duration().TotalSeconds < 1.0);
        }
    }
}
