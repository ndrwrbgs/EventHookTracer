namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using System.Collections.Generic;

    public sealed class LogEventArgs : EventArgs
    {
        public DateTimeOffset Timestamp { get; }
        public IEnumerable<KeyValuePair<string, object>> Fields { get; }

        public LogEventArgs(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.Timestamp = timestamp;
            this.Fields = fields;
        }
    }
}