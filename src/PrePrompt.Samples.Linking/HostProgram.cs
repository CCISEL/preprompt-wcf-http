using System;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Linking
{
    class HostProgram
    {
        static void Main()
        {
            var config = new HttpHostConfiguration()
                .EnableWebLinking(reg => reg.AddLinkFrom<TheService>().To<AnotherService>("next"));
            using (var host = new WebHttpServiceHost(typeof(TheService), config, new Uri("Http://localhost:8080/linking/")))
            {
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", host.BaseAddresses[0]);
                Console.ReadLine();
            }
        }
    }
}