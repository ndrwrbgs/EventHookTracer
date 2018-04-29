namespace OpenTracing.Contrib.EventHookTracer
{
    using System;

    using JetBrains.Annotations;

    internal sealed class EventHookSpanBuilder : ISpanBuilder
    {
        [NotNull] private readonly EventHookTracer tracer;
        private readonly ISpanBuilder impl;

        public EventHookSpanBuilder([NotNull] ISpanBuilder impl, [NotNull] EventHookTracer tracer)
        {
            this.impl = impl;
            this.tracer = tracer;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            ISpanBuilder builder = this.impl.AsChildOf(parent);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            ISpanBuilder builder = this.impl.AsChildOf(parent);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            ISpanBuilder builder = this.impl.AddReference(referenceType, referencedContext);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            ISpanBuilder builder = this.impl.IgnoreActiveSpan();
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            ISpanBuilder builder = this.impl.WithTag(key, value);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset timestamp)
        {
            ISpanBuilder builder = this.impl.WithStartTimestamp(timestamp);
            return new EventHookSpanBuilder(builder, this.tracer);
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            // Cannot call this.impl.StartActive(finishSpanOnDispose) directly due to the lack
            // of exposing of finishSpanOnDispose in IScope (see EventHookScopeManager for details)
            return this.tracer.ScopeManager.Activate(this.impl.Start(), finishSpanOnDispose);
        }

        public ISpan Start()
        {
            ISpan span = this.impl.Start();
            return new EventHookSpan(span, this.tracer);
        }
    }
}