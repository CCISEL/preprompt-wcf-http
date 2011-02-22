using System.Collections.Generic;
using System.Reflection;

namespace PrePrompt.Samples.Common
{
    public class WebLinkCollection
    {
        public WebLinkCollection(MethodInfo method, IList<WebLinkTarget> links = null)
        {
            Method = method;
            Links = links ?? new List<WebLinkTarget>();
        }

        public MethodInfo Method { get; private set; }
        public IList<WebLinkTarget> Links { get; private set; }
    }
}