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
    class GetTimeForZoneTests
    {
        [Test]
        public void GetTimeForZoneReturnsOkWithExistingZone()
        {
            var service = new TheService();
            var req = new HttpRequestMessage();
            var resp = new HttpResponseMessage();
            service.GetTimeForZone(req, resp, "Azores Standard Time");
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }

        [Test]
        public void GetTimeForZoneReturnsNotFoundWithUnexistingZone()
        {
            var service = new TheService();
            var req = new HttpRequestMessage();
            var resp = new HttpResponseMessage();
            service.GetTimeForZone(req, resp, "Unexistant zone");
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }
    }
}
