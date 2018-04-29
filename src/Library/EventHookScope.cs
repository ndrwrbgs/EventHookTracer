namespace OpenTracing.Contrib.EventHookTracer
{
    using JetBrains.Annotations;

    internal sealed class EventHookScope : IScope
    {
        private readonly bool finishSpanOnDispose;
        [NotNull] private readonly EventHookTracer tracer;
        private readonly IScope impl;

        public EventHookScope(
            [NotNull] IScope impl,
            [NotNull] EventHookTracer tracer,
            bool finishSpanOnDispose)
        {
            this.impl = impl;
            this.tracer = tracer;
            this.finishSpanOnDispose = finishSpanOnDispose;
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
                var wrap = new EventHookSpan(span, this.tracer);
                return wrap;
            }
        }
    }
}