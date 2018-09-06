namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;

    internal sealed class EventHookSpan : ISpan
    {
        internal readonly ISpan _spanImplementation;
        private readonly EventHookTracer tracer;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;

        public EventHookSpan(
            ISpan span,
            EventHookTracer tracer,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag)
        {
            this._spanImplementation = span;
            this.tracer = tracer;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
        }

        public ISpan SetTag(string key, string value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan SetTag(string key, bool value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan SetTag(string key, int value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan SetTag(string key, double value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan Log(IDictionary<string, object> fields)
        {
            ISpan span = this._spanImplementation.Log(fields);
            this.spanLog(this, new EventHookTracer.LogEventArgs(DateTimeOffset.UtcNow, fields));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            ISpan span = this._spanImplementation.Log(timestamp, fields);
            this.spanLog(this, new EventHookTracer.LogEventArgs(timestamp, fields));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan Log(string @event)
        {
            ISpan span = this._spanImplementation.Log(@event);
            this.spanLog(
                this,
                new EventHookTracer.LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            ISpan span = this._spanImplementation.Log(timestamp, @event);
            this.spanLog(
                this,
                new EventHookTracer.LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            ISpan span = this._spanImplementation.SetBaggageItem(key, value);
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public string GetBaggageItem(string key)
        {
            return this._spanImplementation.GetBaggageItem(key);
        }

        public ISpan SetOperationName(string operationName)
        {
            ISpan span = this._spanImplementation.SetOperationName(operationName);
            return new EventHookSpan(span, this.tracer, this.spanLog, this.spanSetTag);
        }

        public void Finish()
        {
            this.tracer.OnSpanFinished(this._spanImplementation);
            this._spanImplementation.Finish();
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            this.tracer.OnSpanFinished(this._spanImplementation);
            this._spanImplementation.Finish(finishTimestamp);
        }

        public ISpanContext Context => this._spanImplementation.Context;
    }
}