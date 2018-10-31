namespace OpenTracing.Contrib.EventHookTracer
{
    using System;

    public sealed class SetTagEventArgs : EventArgs
    {
        public string Key { get; }
        public object Value { get; }

        public SetTagEventArgs(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}