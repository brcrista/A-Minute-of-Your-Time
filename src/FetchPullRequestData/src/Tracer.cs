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
    }
}