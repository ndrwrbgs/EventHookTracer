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
    using OpenTracing.Contrib.MutableTracer;
    using OpenTracing.Util;

    public sealed class EventHookScopeManager : StronglyTypedScopeManager<EventHookScope, EventHookSpan>
    {
#if NET45 // AsyncLocal is .NET 4.6+, so fall back to CallContext for .NET 4.5
        private sealed class AsyncLocal<T>
        {
            private readonly string logicalDataKey = "__AsyncLocal_" + Guid.NewGuid();

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

        private readonly EventHookTracer tracer;
        private readonly EventHandler<LogEventArgs> spanLog;
        private readonly EventHandler<SetTagEventArgs> spanSetTag;

        /// <summary>
        ///     See <see cref="Active" /> for why this exists :(
        /// </summary>
        private readonly AsyncLocal<EventHookScope> activeScope = new AsyncLocal<EventHookScope>();

        public EventHookScopeManager([NotNull] EventHookTracer tracer, EventHandler<LogEventArgs> spanLog, EventHandler<SetTagEventArgs> spanSetTag)
        {
            this.tracer = tracer;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
        }

        public override EventHookScope Activate(EventHookSpan eventHookSpan, bool finishSpanOnDispose)
        {
            this.tracer.OnSpanActivating(eventHookSpan);

            var previousActive = activeScope.Value;
            var wrap = new EventHookScope(this.tracer, eventHookSpan, finishSpanOnDispose ? () => { activeScope.Value = previousActive; } : (Action)null);
            activeScope.Value = wrap;

            // Perform the one-time-activation logic (like logging tags)
            if (eventHookSpan.onActivated != null)
            {
                eventHookSpan.onActivated(eventHookSpan);
                // Set to null (because we want one-time-activation)
                eventHookSpan.onActivated = null;
            }
            
            this.tracer.OnSpanActivated(eventHookSpan);

            return wrap;
        }

        /// <remarks>
        ///     <see cref="IScope" /> does not expose its property finishOnDispose, and there's no implementation agnostic way to
        ///     intercept the Finish on the <see cref="ISpan" />. This leaves the only option of implementing in this the scope
        ///     management.
        /// </remarks>
        public override EventHookScope Active
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