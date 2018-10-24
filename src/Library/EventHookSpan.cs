namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using OpenTracing.Contrib.MutableTracer;
    using OpenTracing.Noop;
    using OpenTracing.Tag;

    public sealed class EventHookSpan : StronglyTypedSpan<EventHookSpan, ISpanContext>
    {
        private readonly EventHookTracer tracer;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;
        internal Action<EventHookSpan> onActivated { get; set; }

        public string OperationName { get; }

        public EventHookSpan(
            EventHookTracer tracer,
            string operationName,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag,
            Action<EventHookSpan> onActivated)
        {
            this.OperationName = operationName;
            this.tracer = tracer;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.onActivated = onActivated;
        }

        public override EventHookSpan SetTag(BooleanTag tag, bool value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetTag(IntOrStringTag tag, string value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetTag(IntTag tag, int value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetTag(StringTag tag, string value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, string value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, bool value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, int value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, double value)
        {
            this.spanSetTag(this, new EventHookTracer.SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.spanLog(this, new EventHookTracer.LogEventArgs(DateTimeOffset.UtcNow, fields));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.spanLog(this, new EventHookTracer.LogEventArgs(timestamp, fields));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan Log(string @event)
        {
            this.spanLog(
                this,
                new EventHookTracer.LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan Log(DateTimeOffset timestamp, string @event)
        {
            this.spanLog(
                this,
                new EventHookTracer.LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override EventHookSpan SetBaggageItem(string key, string value)
        {
            // TODO: Not baggage support - is that allowed?
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override string GetBaggageItem(string key)
        {
            // TODO: Not baggage support - is that allowed?
            return null;
        }

        public override EventHookSpan SetOperationName(string operationName)
        {
            return new EventHookSpan(this.tracer, operationName, this.spanLog, this.spanSetTag, this.onActivated);
        }

        public override void Finish()
        {
            this.tracer.OnSpanFinishing(this);
            // They should be calling Dispose on the scope generally, but if they've manually Finished the span, go ahead and NOOP fire the events
            this.tracer.OnSpanFinished(this);
        }

        public override void Finish(DateTimeOffset finishTimestamp)
        {
            // TODO: Not passing timestamp
            this.tracer.OnSpanFinishing(this);
            // They should be calling Dispose on the scope generally, but if they've manually Finished the span, go ahead and NOOP fire the events
            this.tracer.OnSpanFinished(this);
        }

        public override ISpanContext Context
        {
            get
            {
                // TODO: Is it required we return a non-null impl here?
                //// return null;
                return NoopTracerFactory.Create().ActiveSpan.Context; // Cannot directly access NoopSpanContext's ctor
            }
        }
    }
}