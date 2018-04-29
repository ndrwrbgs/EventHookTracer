namespace OpenTracing.Contrib.EventHookTracer
{
    using System;

    using JetBrains.Annotations;

    using OpenTracing.Propagation;

    public sealed class EventHookTracer : ITracer
    {
        private readonly ITracer impl;

        public EventHookTracer([NotNull] ITracer impl)
        {
            this.impl = impl;
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            ISpanBuilder builder = this.impl.BuildSpan(operationName);
            return new EventHookSpanBuilder(builder, this);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            this.impl.Inject(spanContext, format, carrier);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return this.impl.Extract(format, carrier);
        }

        public IScopeManager ScopeManager
        {
            get
            {
                IScopeManager scopeManager = this.impl.ScopeManager;
                return new EventHookScopeManager(scopeManager, this);
            }
        }

        public ISpan ActiveSpan
        {
            get
            {
                ISpan span = this.impl.ActiveSpan;
                return new EventHookSpan(span, this);
            }
        }

        public event EventHandler<ISpan> SpanActivated = delegate { };
        public event EventHandler<ISpan> SpanFinished = delegate { };

        internal void OnSpanActivated(ISpan span)
        {
            ISpan underlyingSpan = span;
            if (underlyingSpan is EventHookSpan eventHookSpan)
            {
                underlyingSpan = eventHookSpan._spanImplementation;
            }

            this.SpanActivated(this, underlyingSpan);
        }

        internal void OnSpanFinished(ISpan span)
        {
            ISpan underlyingSpan = span;
            if (underlyingSpan is EventHookSpan eventHookSpan)
            {
                underlyingSpan = eventHookSpan._spanImplementation;
            }

            this.SpanFinished(this, underlyingSpan);
        }
    }
}