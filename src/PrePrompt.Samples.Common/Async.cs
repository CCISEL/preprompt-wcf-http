using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Threading.Tasks;

namespace PrePrompt.Samples.Common
{
    public static class Async
    {
        public static HttpOperationDescription EnableAsync(this HttpOperationDescription operation)
        {
            var retValue = operation.ReturnValue;
            if (typeof(Task).IsAssignableFrom(retValue.ParameterType))
            {
                operation.Behaviors.Add(new AsyncOperationBehavior());
                retValue.ParameterType = ReflectionHelper.GetFutureResultType(retValue.ParameterType) ?? typeof(void);
            }
            return operation;
        }
    }

    public class AsyncOperationBehavior : IOperationBehavior
    {
        public void ApplyDispatchBehavior(OperationDescription description, DispatchOperation operation)
        {
            operation.Invoker = new AsyncTaskOperationInvoker(description.SyncMethod, operation.Invoker);
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        { }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        { }

        public void Validate(OperationDescription operationDescription)
        { }
    }

    internal class AsyncTaskOperationInvoker : IOperationInvoker
    {
        private static readonly object[] _emptyArray = new object[0];

        private readonly MethodInfo _method;
        private readonly IOperationInvoker _inner;
        private readonly MethodInfo _resultGetter;

        public AsyncTaskOperationInvoker(MethodInfo method, IOperationInvoker inner)
        {
            _method = method;
            _inner = inner;
            _resultGetter = ReflectionHelper.GetFutureResultGetMethod(method.ReturnType);
        }

        public bool IsSynchronous
        {
            get { return false; }
        }

        public object[] AllocateInputs()
        {
            return _inner.AllocateInputs();
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            var task = _method.Invoke(instance, inputs) as Task;
            Debug.Assert(task != null);
            var arw = new AsyncResultWrapper(task, state);
            if (callback != null)
            {
                task.ContinueWith(_ => callback(arw));
            }
            return arw;
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            outputs = new object[0];
            var arw = result as AsyncResultWrapper;
            Debug.Assert(arw != null);
            return _resultGetter != null ? _resultGetter.Invoke(arw.Inner, _emptyArray) : null;
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            throw new NotSupportedException();
        }
    }

    public class AsyncResultWrapper : IAsyncResult
    {
        private readonly object _state;
        private readonly IAsyncResult _inner;

        public AsyncResultWrapper(IAsyncResult inner, object state)
        {
            _inner = inner;
            _state = state;
        }

        public IAsyncResult Inner
        {
            get { return _inner; }
        }

        public object AsyncState
        {
            get { return _state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _inner.AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return _inner.CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return _inner.IsCompleted; }
        }
    }
}
