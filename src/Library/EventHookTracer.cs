namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using OpenTracing.Contrib.MutableTracer;
    using OpenTracing.Noop;
    using OpenTracing.Propagation;

    public sealed class EventHookTracer : StronglyTypedTracer<EventHookSpanBuilder, EventHookSpanContext, EventHookScopeManager, EventHookSpan>
    {
        public EventHookTracer()
        {
            this.ScopeManager = new EventHookScopeManager(this, this.SpanLog, this.SpanSetTag);
        }

        public override EventHookSpanBuilder BuildSpan(string operationName)
        {
            return new EventHookSpanBuilder(this, operationName, this.SpanLog, this.SpanSetTag, ImmutableList<SetTagEventArgs>.Empty, null, false);
        }

        public override void Inject<TCarrier>(EventHookSpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
        }

        public override EventHookSpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            // TODO: Is it required we return a non-null impl here?
            return null;
            //return new EventHookSpanContext();
        }

        public override EventHookScopeManager ScopeManager { get; }

        public override EventHookSpan ActiveSpan
        {
            get
            {
                var activeScope = this.ScopeManager.Active;

                if (activeScope == null)
                {
                    return null;
                }

                return activeScope.Span;
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
            this.SpanActivated(this, new BasicSpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }

        internal void OnSpanActivating(EventHookSpan eventHookSpan)
        {
            this.SpanActivating(this, new BasicSpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }

        internal void OnSpanFinished(EventHookSpan eventHookSpan)
        {
            this.SpanFinished(this, new BasicSpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }

        internal void OnSpanFinishing(EventHookSpan eventHookSpan)
        {
            this.SpanFinishing(this, new BasicSpanLifecycleEventArgs(eventHookSpan, eventHookSpan.OperationName));
        }
    }
}