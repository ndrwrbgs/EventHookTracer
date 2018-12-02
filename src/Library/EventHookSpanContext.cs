using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTracing.Contrib.EventHookTracer
{
    using System.Collections.Immutable;

    using OpenTracing.Contrib.MutableTracer;

    public sealed class EventHookSpanContext : ISpanContext
    {
        public ImmutableDictionary<string, string> Dictionary { get; internal set; } = ImmutableDictionary<string, string>.Empty;

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return Dictionary;
        }

        public string TraceId { get; }
        public string SpanId { get; }
    }
}
