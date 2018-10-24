namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using OpenTracing.Tag;

    internal sealed class EventHookSpan : ISpan
    {
        internal readonly ISpan _spanImplementation;
        private readonly EventHookTracer tracer;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;
        internal Action<EventHookSpan> onActivated { get; set; }

        public string OperationName { get; }

        public EventHookSpan(
            ISpan span,
            EventHookTracer tracer,
            string operationName,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag,
            Action<EventHookSpan> onActivated)
        {
            this._spanImplementation = span;
            this.OperationName = operationName;
            this.tracer = tracer;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.onActivated = onActivated;
        }

        public ISpan SetTag(BooleanTag tag, bool value)
        {
            ISpan span = this._spanImplementation.SetTag(tag, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetTag(IntOrStringTag tag, string value)
        {
            ISpan span = this._spanImplementation.SetTag(tag, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetTag(IntTag tag, int value)
        {
            ISpan span = this._spanImplementation.SetTag(tag, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetTag(StringTag tag, string value)
        {
            ISpan span = this._spanImplementation.SetTag(tag, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetTag(string key, string value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetTag(string key, bool value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetTag(string key, int value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetTag(string key, double value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            ISpan span = this._spanImplementation.Log(fields);
            this.spanLog(this, new EventHookTracer.LogEventArgs(DateTimeOffset.UtcNow, fields));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            ISpan span = this._spanImplementation.Log(timestamp, fields);
            this.spanLog(this, new EventHookTracer.LogEventArgs(timestamp, fields));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan Log(string @event)
        {
            ISpan span = this._spanImplementation.Log(@event);
            this.spanLog(
                this,
                new EventHookTracer.LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            ISpan span = this._spanImplementation.Log(timestamp, @event);
            this.spanLog(
                this,
                new EventHookTracer.LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            ISpan span = this._spanImplementation.SetBaggageItem(key, value);
            return new EventHookSpan(span, this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public string GetBaggageItem(string key)
        {
            return this._spanImplementation.GetBaggageItem(key);
        }

        public ISpan SetOperationName(string operationName)
        {
            ISpan span = this._spanImplementation.SetOperationName(operationName);
            return new EventHookSpan(span, this.tracer, operationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public void Finish()
        {
            this.tracer.OnSpanFinishing(this);
            this._spanImplementation.Finish();
            this.tracer.OnSpanFinished(this);
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            this.tracer.OnSpanFinishing(this);
            this._spanImplementation.Finish(finishTimestamp);
            this.tracer.OnSpanFinished(this);
        }

        public ISpanContext Context => this._spanImplementation.Context;
    }
}