using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace PrePrompt.Samples.SoapModel
{
    [DataContract(Namespace="http://www.cc.isel.ipl.pt")]
    public class GetTimeRequest
    {
        [DataMember]
        public String Zone { get; set; }
    }

    [DataContract(Namespace = "http://www.cc.isel.ipl.pt")]
    public class GetTimeResponse
    {
        [DataMember]
        public String Time { get; set; }
    }

    [ServiceContract(Name="TheService",Namespace = "http://www.cc.isel.ipl.pt/")]
    public interface ITheServiceContract
    {
        [OperationContract]
        GetTimeResponse GetTime(GetTimeRequest req);
    }

    class TheServiceImpl : ITheServiceContract
    {
        public GetTimeResponse GetTime(GetTimeRequest req)
        {
            if (req.Zone == null)
            {
                return new GetTimeResponse() { Time = DateTime.Now.ToLongTimeString() };
            }
            try
            {
                var zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(req.Zone);
                return new GetTimeResponse()
                           {
                               Time = TimeZoneInfo.ConvertTime(DateTime.Now, zoneInfo).ToLongTimeString()
                           };
            }
            catch (TimeZoneNotFoundException e)
            {
                throw new FaultException(e.Message);
            }
        }
    }

    class HostProgram
    {
        static void Main(string[] args)
        {
            using(var host = new ServiceHost(typeof(TheServiceImpl)))
            {
                host.AddServiceEndpoint(
                    typeof (ITheServiceContract),
                    new BasicHttpBinding(),
                    "http://localhost:8080/soap/time");

                host.Description.Behaviors.Add(new ServiceMetadataBehavior
                                                   {
                                                       HttpGetEnabled = true,
                                                       HttpGetUrl = new Uri("http://localhost:8080/soap/time/wsdl")
                                                   });

                host.Open();
                Console.WriteLine("Service is open, press any key to end");
                TheClient.Run();
                Console.ReadKey();
            }
        }
    }

    class TheClient
    {
        public static void Run()
        {
            using(var cf = new ChannelFactory<ITheServiceContract>(new BasicHttpBinding(), "http://localhost:8080/soap/time"))
            {
                Console.WriteLine(cf.CreateChannel().GetTime(new GetTimeRequest()).Time);
            }
        }
    }

}
