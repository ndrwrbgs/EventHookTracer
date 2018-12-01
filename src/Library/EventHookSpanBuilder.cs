namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using OpenTracing.Contrib.MutableTracer;
    using OpenTracing.Tag;

    // TODO: Don't need to cascadingly make public as long as the tracer itself is hidden behind an interface
    public sealed class EventHookSpanBuilder : StronglyTypedSpanBuilder<EventHookSpanBuilder, EventHookSpanContext, EventHookSpan, EventHookScope>
    {
        [NotNull] private readonly EventHookTracer tracer;
        private readonly string operationName;
        private readonly EventHandler<LogEventArgs> spanLog;
        private readonly EventHandler<SetTagEventArgs> spanSetTag;
        private readonly IList<SetTagEventArgs> tagsOnStart;
        private readonly EventHookSpanContext parentSpanContext;

        public EventHookSpanBuilder(
            [NotNull] EventHookTracer tracer,
            string operationName,
            EventHandler<LogEventArgs> spanLog,
            EventHandler<SetTagEventArgs> spanSetTag,
            IList<SetTagEventArgs> tagsOnStart,
            EventHookSpanContext parentSpanContext)
        {
            this.tracer = tracer;
            this.operationName = operationName;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.tagsOnStart = tagsOnStart;
            this.parentSpanContext = parentSpanContext;
        }


        public override EventHookSpanBuilder AsChildOf(EventHookSpanContext parent)
        {
            return new EventHookSpanBuilder(
                this.tracer,
                this.operationName,
                this.spanLog,
                this.spanSetTag,
                this.tagsOnStart,
                parent);
        }

        public override EventHookSpanBuilder AsChildOf(EventHookSpan parent)
        {
            return new EventHookSpanBuilder(
                this.tracer,
                this.operationName,
                this.spanLog,
                this.spanSetTag,
                this.tagsOnStart,
                parent.Context);
        }

        public override EventHookSpanBuilder AddReference(string referenceType, EventHookSpanContext referencedContext)
        {
            // TODO: Not tracking children presently - relying only on 'current active' in OnActivating
            ////return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
            return this;
        }

        public override EventHookSpanBuilder IgnoreActiveSpan()
        {
            // TODO: Not tracking children presently - relying only on 'current active' in OnActivating
            ////return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
            return this;
        }

        public override EventHookSpanBuilder WithTag(BooleanTag tag, bool value)
        {
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithTag(IntTag tag, int value)
        {
            // TODO: use something else, List is not the most efficient here
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithTag(StringTag tag, string value)
        {
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithTag(string key, string value)
        {
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithTag(string key, bool value)
        {
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithTag(string key, int value)
        {
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithTag(string key, double value)
        {
            var newTags = new List<SetTagEventArgs>(this.tagsOnStart)
                {new SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags, this.parentSpanContext);
        }

        public override EventHookSpanBuilder WithStartTimestamp(DateTimeOffset timestamp)
        {
            // TODO: Not currently tracking specific start times
            ////return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
            return this;
        }

        public override EventHookScope StartActive(bool finishSpanOnDispose)
        {
            // Cannot call this.impl.StartActive(finishSpanOnDispose) directly due to the lack
            // of exposing of finishSpanOnDispose in IScope (see EventHookScopeManager for details)
            return this.tracer.ScopeManager.Activate(this.Start(), finishSpanOnDispose);
        }

        public override EventHookScope StartActive()
        {
            // Cannot call this.impl.StartActive(finishSpanOnDispose) directly due to the lack
            // of exposing of finishSpanOnDispose in IScope (see EventHookScopeManager for details)
            return this.tracer.ScopeManager.Activate(this.Start(), true);
        }

        public override EventHookSpan Start()
        {
            // TODO: IgnoreActive support
            var currentSpan = this.tracer.ScopeManager.Active?.Span;
            var parent = this.parentSpanContext;
            if (currentSpan != null)
            {
                parent = currentSpan.Context;
            }

            // this.impl.Start() above will(/should) internally all SetTag for each of the specified tags.
            // unfortunately there's no way in the interface to capture that call (it'll call it on it's concrete type)
            // so we must pseudo-replicate the behavior here.
            return new EventHookSpan(this.tracer, this.operationName, this.spanLog, this.spanSetTag,
                                     parent,
                                     ownsContext: false,
                onActivated: s =>
                {
                    foreach (var tagOnStart in this.tagsOnStart)
                    {
                        this.spanSetTag(s, tagOnStart);
                    }
                });
        }
    }
}