using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Microsoft.ServiceModel.Http;

namespace PrePrompt.Samples.Common
{
    public static class WebLinking
    {
        public static HttpHostConfiguration EnableWebLinking(this HttpHostConfiguration configuration, 
                                                             Action<WebLinksRegistry> action = null)
        {
            var registry = configuration.ProcessorProvider is WebLinkingProcessorProvider
                         ? ((WebLinkingProcessorProvider)configuration.ProcessorProvider).LinksRegistry
                         : WebLinksRegistry.From(configuration);
            
            if (action != null)
            {
                action(registry);
            }
            
            return configuration;
        }
    }

    public class WebLinksRegistry
    {
        internal readonly Dictionary<MethodInfo, HashSet<WebLinkTarget>> Links =
            new Dictionary<MethodInfo, HashSet<WebLinkTarget>>();

        private WebLinksRegistry()
        { }

        public static WebLinksRegistry From(HttpHostConfiguration configuration)
        {
            var registry = new WebLinksRegistry();
            var processorProvider = new WebLinkingProcessorProvider(registry, configuration.ProcessorProvider);
            configuration.SetProcessorProvider(processorProvider);
            return registry;
        }

        public WebLinkCollection GetLinksFor(MethodInfo method)
        {
            HashSet<WebLinkTarget> linkCollection;
            return Links.TryGetValue(method, out linkCollection)
                 ? new WebLinkCollection(method, linkCollection.Select(target => target.Clone()).ToList())
                 : new WebLinkCollection(method);
        }

        public WebLinkConfiguration AddLinkFrom<TResource>(Expression<Action<TResource>> methodSelector = null)
        {
            return new WebLinkConfiguration(this, getTarget(methodSelector).Item1);
        }

        internal Tuple<MethodInfo, string> getTarget<TResource>(Expression<Action<TResource>> methodSelector)
        {
            ensureIsResource<TResource>();
            return methodSelector != null ? null : getGetMethodFor(typeof(TResource));
        }

        internal static void ensureIsResource<T>()
        {
            if (ContractDescription.GetContract(typeof(T)) == null)
            {
                throw new InvalidOperationException("The specified type must implement the ServiceContract attribute");
            }
        }

        private static Tuple<MethodInfo, string> getGetMethodFor(Type resourceType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public;
            var context = resourceType
                .GetMethods(flags)
                .Where(m => m.HasAttribute<WebGetAttribute>())
                .Select(m => Tuple.Create(m, m.Attribute<WebGetAttribute>().UriTemplate))
                .FirstOrDefault();

            if (context == null)
            {
                throw new ArgumentException("The specified resource must allow the HTTP GET method");
            }

            return context;
        }
    }

    public class WebLinkConfiguration
    {
        private readonly WebLinksRegistry _registry;
        private readonly MethodInfo _context;

        public WebLinkConfiguration(WebLinksRegistry registry, MethodInfo context)
        {
            _registry = registry;
            _context = context;
        }

        public WebLinksRegistry To<TResource>(string relType, Expression<Action<TResource>> methodSelector = null)
        {
            var target = _registry.getTarget(methodSelector);
            HashSet<WebLinkTarget> targets;
            if (_registry.Links.TryGetValue(_context, out targets) == false)
            {
                _registry.Links.Add(_context, (targets = new HashSet<WebLinkTarget>()));
            }
            targets.Add(new WebLinkTarget(target.Item1, target.Item2, relType));
            return _registry;
        }
    }
}