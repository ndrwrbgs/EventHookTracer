namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using OpenTracing.Contrib.MutableTracer;
    using OpenTracing.Noop;
    using OpenTracing.Tag;

    public sealed class EventHookSpan : StronglyTypedSpan<EventHookSpan, EventHookSpanContext>, IEquatable<EventHookSpan>
    {
        private readonly EventHookTracer tracer;
        private readonly EventHandler<LogEventArgs> spanLog;
        private readonly EventHandler<SetTagEventArgs> spanSetTag;
        internal Action<EventHookSpan> onActivated { get; set; }

        public string OperationName { get; private set; }

        public EventHookSpan(
            EventHookTracer tracer,
            string operationName,
            EventHandler<LogEventArgs> spanLog,
            EventHandler<SetTagEventArgs> spanSetTag,
            EventHookSpanContext context,
            bool ownsContext,
            Action<EventHookSpan> onActivated)
        {
            this.OperationName = operationName;
            this.tracer = tracer;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.onActivated = onActivated;
            this.MyContext = context;
            this.ownsContext = ownsContext;
        }

        public override EventHookSpan SetTag(BooleanTag tag, bool value)
        {
            this.spanSetTag(this, new SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetTag(IntOrStringTag tag, string value)
        {
            this.spanSetTag(this, new SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetTag(IntTag tag, int value)
        {
            this.spanSetTag(this, new SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetTag(StringTag tag, string value)
        {
            this.spanSetTag(this, new SetTagEventArgs(tag.Key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, string value)
        {
            this.spanSetTag(this, new SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, bool value)
        {
            this.spanSetTag(this, new SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, int value)
        {
            this.spanSetTag(this, new SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetTag(string key, double value)
        {
            this.spanSetTag(this, new SetTagEventArgs(key, value));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.spanLog(this, new LogEventArgs(DateTimeOffset.UtcNow, fields));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.spanLog(this, new LogEventArgs(timestamp, fields));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan Log(string @event)
        {
            this.spanLog(
                this,
                new LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan Log(DateTimeOffset timestamp, string @event)
        {
            this.spanLog(
                this,
                new LogEventArgs(
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> {["event"] = @event}));
            // TODO: Most of these can be 'return this'
            return new EventHookSpan(this.tracer, this.OperationName, this.spanLog, this.spanSetTag, this.Context, this.ownsContext, this.onActivated);
        }

        public override EventHookSpan SetBaggageItem(string key, string value)
        {
            // TODO: For this to be proper, will need a copy-on-write dictionary
            if (!this.ownsContext)
            {
                if (this.myContext == null)
                {
                    this.MyContext = new EventHookSpanContext
                    {
                        Dictionary = new Dictionary<string, string>()
                    };
                }
                else
                {
                    this.MyContext = new EventHookSpanContext
                    {
                        Dictionary = new Dictionary<string, string>(this.myContext.Dictionary)
                    };
                }

                this.ownsContext = true;
            }

            this.Context.Dictionary[key] = value;
            return this;
        }

        public override string GetBaggageItem(string key)
        {
            return this.Context.Dictionary.TryGetValue(key, out string value) ? value : null;
        }

        public override EventHookSpan SetOperationName(string operationName)
        {
            this.OperationName = operationName;
            return this;
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

        // TODO: Can remove once we have copy-on-write
        private bool ownsContext = false;
        private EventHookSpanContext myContext;

        private EventHookSpanContext MyContext
        {
            get
            {
                if (this.myContext == null)
                {
                    this.myContext = new EventHookSpanContext();
                    this.ownsContext = true;
                }

                return this.myContext;
            }
            set { this.myContext = value; }
        }
        public override EventHookSpanContext Context => MyContext;

        public bool Equals(EventHookSpan other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this.tracer, other.tracer) && Equals(this.spanLog, other.spanLog) && Equals(this.spanSetTag, other.spanSetTag) && Equals(this.onActivated, other.onActivated) && string.Equals(this.OperationName, other.OperationName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is EventHookSpan other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (this.tracer != null ? this.tracer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.spanLog != null ? this.spanLog.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.spanSetTag != null ? this.spanSetTag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.onActivated != null ? this.onActivated.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.OperationName != null ? this.OperationName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(EventHookSpan left, EventHookSpan right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventHookSpan left, EventHookSpan right)
        {
            return !Equals(left, right);
        }
    }
}