namespace OpenTracing.Contrib.EventHookTracer
{
#if NET451 // AsyncLocal is .NET 4.6+, so fall back to CallContext for .NET 4.5
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
#else
    using System.Threading;
#endif

    using JetBrains.Annotations;

    internal sealed class EventHookScopeManager : IScopeManager
    {
#if NET451 // AsyncLocal is .NET 4.6+, so fall back to CallContext for .NET 4.5
        private sealed class AsyncLocal<T>
        {
            private static readonly string logicalDataKey = "__AsyncLocal_" + Guid.NewGuid();

            public T Value
            {
                get
                {
                    var handle = CallContext.LogicalGetData(logicalDataKey) as ObjectHandle;
                    return (T) handle?.Unwrap();
                }
                set
                {
                    CallContext.LogicalSetData(logicalDataKey, new ObjectHandle(value));
                }
            }
        }
#endif

        private readonly IScopeManager impl;
        private readonly EventHookTracer tracer;

        /// <summary>
        ///     See <see cref="Active" /> for why this exists :(
        /// </summary>
        private readonly AsyncLocal<EventHookScope> activeScope = new AsyncLocal<EventHookScope>();

        public EventHookScopeManager([NotNull] IScopeManager impl, [NotNull] EventHookTracer tracer)
        {
            this.impl = impl;
            this.tracer = tracer;
        }

        public IScope Activate(ISpan span, bool finishSpanOnDispose)
        {
            this.tracer.OnSpanActivated(span);

            // It's likely this got the span from the eventhook, the underlying tracer should be given the same type it makes though
            if (span is EventHookSpan eventHookSpan)
            {
                span = eventHookSpan._spanImplementation;
            }

            IScope scope = this.impl.Activate(span, finishSpanOnDispose);
            var wrap = new EventHookScope(scope, this.tracer, finishSpanOnDispose);
            this.activeScope.Value = wrap;
            return wrap;
        }

        /// <remarks>
        ///     <see cref="IScope" /> does not expose its property finishOnDispose, and there's no implementation agnostic way to
        ///     intercept the Finish on the <see cref="ISpan" />. This leaves the only option of implementing in this the scope
        ///     management.
        /// </remarks>
        public IScope Active
        {
            get
            {
                return this.activeScope.Value;
                //var scope = this.impl.Active;
                //var wrap = new EventHookScope(scope, this.tracer, );
                //throw new NotImplementedException("Scope does not expose it's Finish/Close state, and there's not an implementation agnostic way to wrap");
            }
        }
    }
}