namespace OpenTracing.Contrib.EventHookTracer
{
    using System;

    public abstract class SpanLifecycleEventArgs : EventArgs
    {
        public ISpan Span { get; }
        public string OperationName { get; }

        public SpanLifecycleEventArgs(ISpan span, string operationName)
        {
            this.Span = span;
            this.OperationName = operationName;
        }
    }

    /// <summary>
    /// For use until concrete types are developed for each scenario
    /// </summary>
    public sealed class BasicSpanLifecycleEventArgs : SpanLifecycleEventArgs
    {
        public BasicSpanLifecycleEventArgs(ISpan span, string operationName)
            : base(span, operationName)
        {
        }
    }

    public sealed class SpanStarting
}