namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using JetBrains.Annotations;
    using OpenTracing.Contrib.MutableTracer;

    public sealed class EventHookScope : StronglyTypedScope<EventHookSpan>
    {
        private readonly bool finishSpanOnDispose;
        private readonly string spanOperationName;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;
        private readonly Action onDispose;
        [NotNull] private readonly EventHookTracer tracer;

        public EventHookScope(
            [NotNull] EventHookTracer tracer,
            bool finishSpanOnDispose,
            string spanOperationName,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag,
            Action onDispose)
        {
            this.tracer = tracer;
            this.finishSpanOnDispose = finishSpanOnDispose;
            this.spanOperationName = spanOperationName;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.onDispose = onDispose;
        }

        public override void Dispose()
        {
            if (this.finishSpanOnDispose)
            {
                this.tracer.OnSpanFinishing(this.Span);
            }

            this.onDispose?.Invoke();

            if (this.finishSpanOnDispose)
            {
                this.tracer.OnSpanFinished(this.Span);
            }
        }

        public override EventHookSpan Span
        {
            get
            {
                var wrap = new EventHookSpan(this.tracer, this.spanOperationName, this.spanLog, this.spanSetTag, null);
                return wrap;
            }
        }
    }
}