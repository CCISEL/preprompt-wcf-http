using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Reflection;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Dispatcher;
using Microsoft.ServiceModel.Http;

namespace PrePrompt.Samples.Common
{
    public class ProcessorProviderFor<T> : IProcessorProvider
    {
        public delegate Processor ProcessorFunc(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode);

        public delegate void RegistrationAction(
            HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode);

        private static RegistrationAction Compose(RegistrationAction a1, RegistrationAction a2)
        {
            return (op, list, mode) =>
                       {
                           a1(op, list, mode);
                           a2(op, list, mode);
                       };
        }

        private static RegistrationAction OnlyOn(RegistrationAction a1, MediaTypeProcessorMode desiredMode)
        {
            return (op, list, mode) =>
                       {
                           if (mode.Equals(desiredMode))
                           {
                               a1(op, list, mode);
                           }
                       };
        }

        private ICollection<RegistrationAction> _forAll = new LinkedList<RegistrationAction>();
        private IDictionary<MethodInfo, RegistrationAction> _forMap = new Dictionary<MethodInfo, RegistrationAction>();
        private IDictionary<String, MethodInfo> _names = new Dictionary<string, MethodInfo>();

        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            foreach(var ra in _forAll)
                ra(operation, processors, mode);
            RegistrationAction action;
            if(_forMap.TryGetValue(operation.SyncMethod, out action))
            {
                action(operation, processors, mode);
            }
        }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            foreach (var ra in _forAll)
                ra(operation, processors, mode);
            RegistrationAction action;
            if (_forMap.TryGetValue(operation.SyncMethod, out action))
            {
                action(operation, processors, mode);
            }
        }

        internal void RegisterInAllOperations(RegistrationAction action)
        {
            _forAll.Add(action);
        }

        internal void RegisterOnOperationNamed(string s, RegistrationAction action)
        {
            MethodInfo mi = _names[s];
            RegisterOnOperation(mi, action);
        }

        internal void RegisterOnOperation(MethodInfo mi, RegistrationAction action)
        {
            RegistrationAction currAction;
            if (_forMap.TryGetValue(mi, out currAction))
            {
                _forMap[mi] = Compose(currAction, action);
            }
            else
            {
                _forMap[mi] = action;
            }
        }
        
        internal void RegisterOnOperation(Expression<Action<T>> expr, RegistrationAction _action)
        {
            var body = (MethodCallExpression)expr.Body;
            RegisterOnOperation(body.Method, _action);
        }

        public UseCtx Use(ProcessorFunc f)
        {
            return new UseCtx(this, (o,l,m)=>l.Add(f(o,l,m)));
        }

        public UseCtx RemoveAll()
        {
            return new UseCtx(this, (o, l, m) => l.ClearMediaTypeProcessors());
        }


        public class UseCtx
        {
            private readonly ProcessorProviderFor<T> _provider;
            private readonly RegistrationAction _f;

            public UseCtx(ProcessorProviderFor<T> ppf, RegistrationAction f)
            {
                _provider = ppf;
                _f = f;
            }

            public ModeCtx OnRequests
            {
                get { return new ModeCtx(_provider, MediaTypeProcessorMode.Request, _f); }
            }

            public ModeCtx OnResponses
            {
                get { return new ModeCtx(_provider, MediaTypeProcessorMode.Response, _f); }
            }

            public ModeCtx OnBothRequestAndResponse
            {
                get { return new ModeCtx(_provider, _f); }
            }
        }

        public class ModeCtx
        {
            private readonly ProcessorProviderFor<T> _provider;
            private readonly RegistrationAction _action;

            public ModeCtx(ProcessorProviderFor<T> provider, MediaTypeProcessorMode mode, RegistrationAction f)
            {
                _provider = provider;
                _action = (o, l, m) => { if (m.Equals(mode)) f(o, l, m); };
            }

            public ModeCtx(ProcessorProviderFor<T> provider, RegistrationAction f)
            {
                _provider = provider;
                _action = (o, l, m) => { f(o, l, m); };
            }

           
            public void OfAllOperations()
            {
                _provider.RegisterInAllOperations(_action);
            }

            public void OfOperationNamed(string s)
            {
                _provider.RegisterOnOperationNamed(s, _action);
            }

            public void OfOperation(Expression<Action<T>> expr)
            {
                _provider.RegisterOnOperation(expr, _action);
            }
        }



        
    }
}
