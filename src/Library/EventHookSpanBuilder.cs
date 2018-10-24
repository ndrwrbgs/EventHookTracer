﻿namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using OpenTracing.Contrib.MutableTracer;
    using OpenTracing.Tag;

    // TODO: Don't need to cascadingly make public as long as the tracer itself is hidden behind an interface
    public sealed class EventHookSpanBuilder : StronglyTypedSpanBuilder<EventHookSpanBuilder, ISpanContext, EventHookSpan, EventHookScope>
    {
        [NotNull] private readonly EventHookTracer tracer;
        private readonly string operationName;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;
        private readonly IList<EventHookTracer.SetTagEventArgs> tagsOnStart;

        public EventHookSpanBuilder(
            [NotNull] EventHookTracer tracer,
            string operationName,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag,
            IList<EventHookTracer.SetTagEventArgs> tagsOnStart)
        {
            this.tracer = tracer;
            this.operationName = operationName;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.tagsOnStart = tagsOnStart;
        }


        public override EventHookSpanBuilder AsChildOf(ISpanContext parent)
        {
            // TODO: Not tracking children presently - relying only on 'current active' in OnActivating
            ////return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
            return this;
        }

        public override EventHookSpanBuilder AsChildOf(EventHookSpan parent)
        {
            // TODO: Not tracking children presently - relying only on 'current active' in OnActivating
            ////return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
            return this;
        }

        public override EventHookSpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
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
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public override EventHookSpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public override EventHookSpanBuilder WithTag(IntTag tag, int value)
        {
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public override EventHookSpanBuilder WithTag(StringTag tag, string value)
        {
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public override EventHookSpanBuilder WithTag(string key, string value)
        {
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public override EventHookSpanBuilder WithTag(string key, bool value)
        {
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public override EventHookSpanBuilder WithTag(string key, int value)
        {
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public override EventHookSpanBuilder WithTag(string key, double value)
        {
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
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
            // this.impl.Start() above will(/should) internally all SetTag for each of the specified tags.
            // unfortunately there's no way in the interface to capture that call (it'll call it on it's concrete type)
            // so we must pseudo-replicate the behavior here.
            return new EventHookSpan(this.tracer, this.operationName, this.spanLog, this.spanSetTag,
                s =>
                {
                    foreach (var tagOnStart in this.tagsOnStart)
                    {
                        this.spanSetTag(s, tagOnStart);
                    }
                });
        }
    }
}