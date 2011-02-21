using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Microsoft.ServiceModel.Http;

namespace PrePrompt.Samples.Common
{
    public static class WebLinking
    {
        public static HttpHostConfiguration ResourceLinks(this HttpHostConfiguration configuration, 
                                                          Action<WebLinksRegistry> action)
        {
            if (configuration.ProcessorProvider is WebLinkingProcessorProvider)
            {
                action(((WebLinkingProcessorProvider)configuration.ProcessorProvider).LinksRegistry);
            }
            else
            {
                action(WebLinksRegistry.From(configuration));
            }
            return configuration;
        }
    }

    public class WebLinksRegistry
    {
        internal readonly ConditionalWeakTable<OperationDescription, HashSet<WebLinkTarget>> Links =
            new ConditionalWeakTable<OperationDescription, HashSet<WebLinkTarget>>();

        private WebLinksRegistry()
        { }

        public static WebLinksRegistry From(HttpHostConfiguration configuration)
        {
            var registry = new WebLinksRegistry();
            var processorProvider = new WebLinkingProcessorProvider(registry, configuration.ProcessorProvider);
            configuration.SetProcessorProvider(processorProvider);
            return registry;
        }

        public WebLinkCollection GetLinksFor(OperationDescription operation)
        {
            return new WebLinkCollection(operation, Links.GetOrCreateValue(operation).Select(target => target.Clone()).ToList());
        }

        public WebLinkConfiguration AddLinkFrom<TResource>(Expression<Action<TResource>> operationSelector = null)
        {
            return new WebLinkConfiguration(this, getTarget(operationSelector).Item1);
        }

        internal Tuple<OperationDescription, string> getTarget<TResource>(Expression<Action<TResource>> operationSelector)
        {
            ensureIsResource<TResource>();
            return operationSelector != null ? null : getGetOperationFor(typeof(TResource));
        }

        internal static void ensureIsResource<T>()
        {
            if (ContractDescription.GetContract(typeof(T)) == null)
            {
                throw new InvalidOperationException("The specified type must implement the ServiceContract attribute");
            }
        }

        private static Tuple<OperationDescription, string> getGetOperationFor(Type resourceType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public;
            var operation = resourceType
                .GetMethods(flags)
                .Select(m => Tuple.Create(m, m.Attribute<WebGetAttribute>()))
                .FirstOrDefault(t => t.Item2 != null);

            if (operation == null)
            {
                throw new ArgumentException("The specified resource must allow the HTTP GET method");
            }

            return Tuple.Create(getOperationFrom(operation.Item1, resourceType), operation.Item2.UriTemplate);
        }

        private static OperationDescription getOperationFrom(MethodInfo methodInfo, Type resourceType)
        {
            //
            // We don't use the MethodInfo.DeclaringType property in order to support inheritance.
            //

            return ContractDescription.GetContract(resourceType).Operations
                .FirstOrDefault(op => op.SyncMethod == methodInfo || op.BeginMethod == methodInfo);
        }
    }

    public class WebLinkConfiguration
    {
        private readonly WebLinksRegistry _registry;
        private readonly OperationDescription _context;

        public WebLinkConfiguration(WebLinksRegistry registry, OperationDescription context)
        {
            _registry = registry;
            _context = context;
        }

        public WebLinksRegistry To<TResource>(string relType, Expression<Action<TResource>> operationSelector = null)
        {
            var target = _registry.getTarget(operationSelector);
            _registry.Links.GetOrCreateValue(_context).Add(new WebLinkTarget(target.Item1, target.Item2, relType));
            return _registry;
        }
    }
}