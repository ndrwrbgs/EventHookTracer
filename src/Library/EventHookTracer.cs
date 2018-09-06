namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    using OpenTracing.Propagation;

    public sealed class EventHookTracer : ITracer
    {
        private readonly ITracer impl;

        public EventHookTracer([NotNull] ITracer impl)
        {
            this.impl = impl;
        }

        ISpanBuilder ITracer.BuildSpan(string operationName)
        {
            return this.BuildSpan(operationName);
        }

        internal EventHookSpanBuilder BuildSpan(string operationName)
        {
            ISpanBuilder builder = this.impl.BuildSpan(operationName);
            return new EventHookSpanBuilder(builder, this, operationName, this.SpanLog, this.SpanSetTag, new List<SetTagEventArgs>());
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            this.impl.Inject(spanContext, format, carrier);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return this.impl.Extract(format, carrier);
        }

        IScopeManager ITracer.ScopeManager => this.ScopeManager;

        internal EventHookScopeManager ScopeManager
        {
            get
            {
                IScopeManager scopeManager = this.impl.ScopeManager;
                return new EventHookScopeManager(scopeManager, this, this.SpanLog, this.SpanSetTag);
            }
        }

        ISpan ITracer.ActiveSpan => this.ActiveSpan;

        internal EventHookSpan ActiveSpan
        {
            get
            {
                ISpan implActive = this.impl.ActiveSpan;

                var believedToBeActive = this.ScopeManager.Active;

                if (!ReferenceEquals(believedToBeActive.Span._spanImplementation, implActive))
                {
                    throw new InvalidOperationException("This Tracer is implemented presuming the currently active span is managed by AsyncLocalScopeManager, but it seems that is not the case.");
                }

                return new EventHookSpan(implActive, this, believedToBeActive.Span.OperationName, this.SpanLog, this.SpanSetTag, null);
            }
        }

        public sealed class SpanLifecycleEventArgs : EventArgs
        {
            public ISpan Span { get; }
            public string OperationName { get; }

            public SpanLifecycleEventArgs(ISpan span, string operationName)
            {
                this.Span = span;
                this.OperationName = operationName;
            }
        }

        public event EventHandler<SpanLifecycleEventArgs> SpanActivated = delegate { };
        public event EventHandler<SpanLifecycleEventArgs> SpanFinished = delegate { };

        public sealed class LogEventArgs : EventArgs
        {
            public DateTimeOffset Timestamp { get; }
            public IEnumerable<KeyValuePair<string, object>> Fields { get; }

            public LogEventArgs(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
            {
                this.Timestamp = timestamp;
                this.Fields = fields;
            }
        }

        public event EventHandler<LogEventArgs> SpanLog = delegate { };

        public sealed class SetTagEventArgs : EventArgs
        {
            public string Key { get; }
            public object Value { get; }

            public SetTagEventArgs(string key, object value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        public event EventHandler<SetTagEventArgs> SpanSetTag = delegate { };

        internal void OnSpanActivated(EventHookSpan eventHookSpan)
        {
            this.SpanActivated(this, new SpanLifecycleEventArgs(eventHookSpan._spanImplementation, eventHookSpan.OperationName));
        }

        internal void OnSpanFinished(EventHookSpan eventHookSpan)
        {
            this.SpanFinished(this, new SpanLifecycleEventArgs(eventHookSpan._spanImplementation, eventHookSpan.OperationName));
        }
    }
}