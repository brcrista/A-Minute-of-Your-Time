using System;

namespace FetchPullRequestData
{
    sealed class Tracer
    {
        private readonly Action<string> trace;

        public Tracer(Action<string> trace)
        {
            this.trace = trace;
        }

        public void Trace(string message) => trace(message + Environment.NewLine);

        public T TraceOperation<T>(string message, Func<T> func)
        {
            trace(message);
            var result = func();
            trace("Done." + Environment.NewLine);

            return result;
        }
    }
}