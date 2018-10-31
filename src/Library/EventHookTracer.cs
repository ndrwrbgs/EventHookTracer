namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using OpenTracing.Contrib.MutableTracer;
    using OpenTracing.Noop;
    using OpenTracing.Propagation;

    public sealed class EventHookTracer : StronglyTypedTracer<EventHookSpanBuilder, ISpanContext, EventHookScopeManager, EventHookSpan>
    {
        public override EventHookSpanBuilder BuildSpan(string operationName)
        {
            return new EventHookSpanBuilder(this, operationName, this.SpanLog, this.SpanSetTag, new List<SetTagEventArgs>());
        }

        public override void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
        }

        public override ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            // TODO: Is it required we return a non-null impl here?
            //// return null;
            return NoopTracerFactory.Create().ActiveSpan.Context; // Cannot directly access NoopSpanContext's ctor
        }

        public override EventHookScopeManager ScopeManager => new EventHookScopeManager(this, this.SpanLog, this.SpanSetTag);

        public override EventHookSpan ActiveSpan
        {
            get
            {
                var activeScope = this.ScopeManager.Active;

                if (activeScope == null)
                {
                    return null;
                }

                return new EventHookSpan(this, activeScope.Span.OperationName, this.SpanLog, this.SpanSetTag, null);
            }
        }


        public event EventHandler<SpanLifecycleEventArgs> SpanActivated = delegate { };
        public event EventHandler<SpanLifecycleEventArgs> SpanActivating = delegate { };
        public event EventHandler<SpanLifecycleEventArgs> SpanFinished = delegate { };
        public event EventHandler<SpanLifecycleEventArgs> SpanFinishing = delegate { };

        public event EventHandler<LogEventArgs> SpanLog = delegate { };

        public event EventHandler<SetTagEventArgs> SpanSetTag = delegate { };

        internal void OnSpanActivated(EventHookSpan eventHookSpan)
        {
            this.SpanActivated(this, new SpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }

        internal void OnSpanActivating(EventHookSpan eventHookSpan)
        {
            this.SpanActivating(this, new SpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }

        internal void OnSpanFinished(EventHookSpan eventHookSpan)
        {
            this.SpanFinished(this, new SpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }

        internal void OnSpanFinishing(EventHookSpan eventHookSpan)
        {
            this.SpanFinishing(this, new SpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }
    }
}