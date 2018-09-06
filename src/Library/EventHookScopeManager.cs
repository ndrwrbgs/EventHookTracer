namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
#if NET45 // AsyncLocal is .NET 4.6+, so fall back to CallContext for .NET 4.5
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
#else
    using System.Threading;
#endif

    using JetBrains.Annotations;

    internal sealed class EventHookScopeManager : IScopeManager
    {
#if NET45 // AsyncLocal is .NET 4.6+, so fall back to CallContext for .NET 4.5
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
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;

        /// <summary>
        ///     See <see cref="Active" /> for why this exists :(
        /// </summary>
        private static readonly AsyncLocal<EventHookScope> activeScope = new AsyncLocal<EventHookScope>();

        public EventHookScopeManager([NotNull] IScopeManager impl, [NotNull] EventHookTracer tracer, EventHandler<EventHookTracer.LogEventArgs> spanLog, EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag)
        {
            this.impl = impl;
            this.tracer = tracer;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
        }

        IScope IScopeManager.Activate(ISpan span, bool finishSpanOnDispose)
        {
            if (span is EventHookSpan eventHookSpan)
            {
                return this.Activate(eventHookSpan, finishSpanOnDispose);
            }

            throw new NotImplementedException("Please only call Activate with Spans that are created by this Tracer.");
            //var eventHookSpan2 = new EventHookSpan(span, this.tracer, /* name unknown */, this.spanLog, this.spanSetTag);
            //return this.Activate(eventHookSpan2, finishSpanOnDispose);
        }

        internal EventHookScope Activate(EventHookSpan eventHookSpan, bool finishSpanOnDispose)
        {
            this.tracer.OnSpanActivated(eventHookSpan);

            // Perform the one-time-activation logic (like logging tags)
            if (eventHookSpan.onActivated != null)
            {
                eventHookSpan.onActivated(eventHookSpan);
                // Set to null (because we want one-time-activation)
                eventHookSpan.onActivated = null;
            }
            
            var span = eventHookSpan._spanImplementation;

            IScope scope = this.impl.Activate(span, finishSpanOnDispose);
            var wrap = new EventHookScope(scope, this.tracer, finishSpanOnDispose, eventHookSpan.OperationName, this.spanLog, this.spanSetTag);
            activeScope.Value = wrap;
            return wrap;
        }

        IScope IScopeManager.Active => this.Active;

        /// <remarks>
        ///     <see cref="IScope" /> does not expose its property finishOnDispose, and there's no implementation agnostic way to
        ///     intercept the Finish on the <see cref="ISpan" />. This leaves the only option of implementing in this the scope
        ///     management.
        /// </remarks>
        internal EventHookScope Active
        {
            get
            {
                return activeScope.Value;
                //var scope = this.impl.Active;
                //var wrap = new EventHookScope(scope, this.tracer, );
                //throw new NotImplementedException("Scope does not expose it's Finish/Close state, and there's not an implementation agnostic way to wrap");
            }
        }
    }
}