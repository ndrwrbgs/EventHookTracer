using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTracing.Contrib.EventHookTracer
{
    using OpenTracing.Contrib.MutableTracer;

    public sealed class EventHookSpanContext : ISpanContext
    {
        // TODO: If CopyOnWrite dictionary, do not need to expose set
        public Dictionary<string, string> Dictionary { get; internal set; } = new Dictionary<string, string>();

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return Dictionary;
        }

        public string TraceId { get; }
        public string SpanId { get; }
    }
}
