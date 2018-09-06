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

        public ISpanBuilder BuildSpan(string operationName)
        {
            ISpanBuilder builder = this.impl.BuildSpan(operationName);
            return new EventHookSpanBuilder(builder, this, this.SpanLog, this.SpanSetTag, new List<SetTagEventArgs>());
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
                return new EventHookScopeManager(scopeManager, this, this.SpanLog, this.SpanSetTag);
            }
        }

        public ISpan ActiveSpan
        {
            get
            {
                ISpan span = this.impl.ActiveSpan;
                return new EventHookSpan(span, this, this.SpanLog, this.SpanSetTag);
            }
        }

        public event EventHandler<ISpan> SpanActivated = delegate { };
        public event EventHandler<ISpan> SpanFinished = delegate { };

        public sealed class LogEventArgs : EventArgs
        {
            public DateTimeOffset Timestamp { get; private set; }
            public IEnumerable<KeyValuePair<string, object>> Fields { get; private set; }

            public LogEventArgs(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
            {
                this.Timestamp = timestamp;
                this.Fields = fields;
            }
        }

        public event EventHandler<LogEventArgs> SpanLog = delegate { };

        public sealed class SetTagEventArgs : EventArgs
        {
            public string Key { get; private set; }
            public object Value { get; private set; }

            public SetTagEventArgs(string key, object value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        public event EventHandler<SetTagEventArgs> SpanSetTag = delegate { };

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