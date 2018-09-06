namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using OpenTracing.Tag;

    internal sealed class EventHookSpanBuilder : ISpanBuilder
    {
        [NotNull] private readonly EventHookTracer tracer;
        private readonly string operationName;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;
        private readonly ISpanBuilder impl;
        private readonly IList<EventHookTracer.SetTagEventArgs> tagsOnStart;

        public EventHookSpanBuilder(
            [NotNull] ISpanBuilder impl,
            [NotNull] EventHookTracer tracer,
            string operationName,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag,
            IList<EventHookTracer.SetTagEventArgs> tagsOnStart)
        {
            this.impl = impl;
            this.tracer = tracer;
            this.operationName = operationName;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.tagsOnStart = tagsOnStart;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            ISpanBuilder builder = this.impl.AsChildOf(parent);
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            ISpanBuilder builder = this.impl.AsChildOf(parent);
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            ISpanBuilder builder = this.impl.AddReference(referenceType, referencedContext);
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            ISpanBuilder builder = this.impl.IgnoreActiveSpan();
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
        }

        public ISpanBuilder WithTag(BooleanTag tag, bool value)
        {
            ISpanBuilder builder = this.impl.WithTag(tag, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            ISpanBuilder builder = this.impl.WithTag(tag, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithTag(IntTag tag, int value)
        {
            ISpanBuilder builder = this.impl.WithTag(tag, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithTag(StringTag tag, string value)
        {
            ISpanBuilder builder = this.impl.WithTag(tag, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(tag.Key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            var newTags = new List<EventHookTracer.SetTagEventArgs>(this.tagsOnStart)
                {new EventHookTracer.SetTagEventArgs(key, value)};
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, newTags);
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset timestamp)
        {
            ISpanBuilder builder = this.impl.WithStartTimestamp(timestamp);
            return new EventHookSpanBuilder(builder, this.tracer, this.operationName, this.spanLog, this.spanSetTag, this.tagsOnStart);
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            // Cannot call this.impl.StartActive(finishSpanOnDispose) directly due to the lack
            // of exposing of finishSpanOnDispose in IScope (see EventHookScopeManager for details)
            return this.tracer.ScopeManager.Activate(this.Start(), finishSpanOnDispose);
        }

        public IScope StartActive()
        {
            // Cannot call this.impl.StartActive(finishSpanOnDispose) directly due to the lack
            // of exposing of finishSpanOnDispose in IScope (see EventHookScopeManager for details)
            return this.tracer.ScopeManager.Activate(this.Start(), true);
        }

        ISpan ISpanBuilder.Start() => this.Start();

        internal EventHookSpan Start()
        {
            ISpan span = this.impl.Start();

            // this.impl.Start() above will(/should) internally all SetTag for each of the specified tags.
            // unfortunately there's no way in the interface to capture that call (it'll call it on it's concrete type)
            // so we must pseudo-replicate the behavior here.
            return new EventHookSpan(span, this.tracer, this.operationName, this.spanLog, this.spanSetTag,
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