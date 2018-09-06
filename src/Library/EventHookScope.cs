namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using JetBrains.Annotations;

    internal sealed class EventHookScope : IScope
    {
        private readonly bool finishSpanOnDispose;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;
        [NotNull] private readonly EventHookTracer tracer;
        private readonly IScope impl;

        public EventHookScope(
            [NotNull] IScope impl,
            [NotNull] EventHookTracer tracer,
            bool finishSpanOnDispose,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag)
        {
            this.impl = impl;
            this.tracer = tracer;
            this.finishSpanOnDispose = finishSpanOnDispose;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
        }

        public void Dispose()
        {
            if (this.finishSpanOnDispose)
            {
                this.tracer.OnSpanFinished(this.Span);
            }

            this.impl.Dispose();
        }

        public ISpan Span
        {
            get
            {
                ISpan span = this.impl.Span;
                var wrap = new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
                return wrap;
            }
        }
    }
}