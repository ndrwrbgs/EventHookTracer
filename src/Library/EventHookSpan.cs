namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;

    internal sealed class EventHookSpan : ISpan
    {
        internal readonly ISpan _spanImplementation;
        private readonly EventHookTracer tracer;

        public EventHookSpan(ISpan span, EventHookTracer tracer)
        {
            this._spanImplementation = span;
            this.tracer = tracer;
        }

        public ISpan SetTag(string key, string value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan SetTag(string key, bool value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan SetTag(string key, int value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan SetTag(string key, double value)
        {
            ISpan span = this._spanImplementation.SetTag(key, value);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan Log(IDictionary<string, object> fields)
        {
            ISpan span = this._spanImplementation.Log(fields);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            ISpan span = this._spanImplementation.Log(timestamp, fields);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan Log(string @event)
        {
            ISpan span = this._spanImplementation.Log(@event);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            ISpan span = this._spanImplementation.Log(timestamp, @event);
            return new EventHookSpan(span, this.tracer);
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            ISpan span = this._spanImplementation.SetBaggageItem(key, value);
            return new EventHookSpan(span, this.tracer);
        }

        public string GetBaggageItem(string key)
        {
            return this._spanImplementation.GetBaggageItem(key);
        }

        public ISpan SetOperationName(string operationName)
        {
            ISpan span = this._spanImplementation.SetOperationName(operationName);
            return new EventHookSpan(span, this.tracer);
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