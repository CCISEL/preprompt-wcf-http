using System;
using System.Threading;

namespace PrePrompt.Samples.Async
{
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