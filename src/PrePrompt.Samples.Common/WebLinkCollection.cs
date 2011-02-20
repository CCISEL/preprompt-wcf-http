using System.Collections.Generic;
using System.ServiceModel.Description;

namespace PrePrompt.Samples.Common
{
    public class WebLinkCollection
    {
        public WebLinkCollection(OperationDescription operation, IList<WebLinkTarget> links = null)
        {
            Operation = operation;
            Links = links ?? new List<WebLinkTarget>();
        }

        public OperationDescription Operation { get; private set; }
        public IList<WebLinkTarget> Links { get; private set; }

        //
        // Provide common queries.
        //
    }
}