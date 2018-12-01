namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using JetBrains.Annotations;
    using OpenTracing.Contrib.MutableTracer;

    public sealed class EventHookScope : StronglyTypedScope<EventHookSpan>
    {
        private readonly bool finishSpanOnDispose;
        private readonly Action onDispose;
        [NotNull] private readonly EventHookTracer tracer;
        private readonly EventHookSpan span;

        public EventHookScope(
            [NotNull] EventHookTracer tracer,
            [NotNull] EventHookSpan span,
            Action onDispose)
        {
            this.tracer = tracer;
            this.span = span;
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
                return this.span;
            }
        }
    }
}