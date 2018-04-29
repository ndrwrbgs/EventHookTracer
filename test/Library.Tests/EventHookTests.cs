using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Library.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using OpenTracing;
    using OpenTracing.Contrib.EventHookTracer;
    using OpenTracing.Mock;

    [TestClass]
    public class EventHookTests
    {
        private ITracer tracer;
        private IList<SpanEvent> events;

        [TestInitialize]
        public void Initialize()
        {
            events = new List<SpanEvent>();

            var eventTracer= new EventHookTracer(new MockTracer());
            eventTracer.SpanActivated += (sender, span) => { this.events.Add(new SpanEvent(span, SpanEventType.Activated)); };
            eventTracer.SpanFinished += (sender, span) => { this.events.Add(new SpanEvent(span, SpanEventType.Finished)); };

            this.tracer = eventTracer;
        }

        [TestMethod]
        public void StandardStartActive()
        {
            IScope outerScope;
            IScope innerScope;
            using (outerScope = this.tracer.BuildSpan("OuterSpan").StartActive(finishSpanOnDispose: true))
            {
                using (innerScope = this.tracer.BuildSpan("InnerSpan").StartActive(finishSpanOnDispose: true))
                {
                    // Do nothing
                }
            }

            CollectionAssert.AreEqual(
                new[]
                {
                    new SpanEvent(outerScope.Span, SpanEventType.Activated),
                    new SpanEvent(innerScope.Span, SpanEventType.Activated),
                    new SpanEvent(innerScope.Span, SpanEventType.Finished),
                    new SpanEvent(outerScope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());
        }

        [TestMethod]
        public void WithSomeModifications()
        {
            IScope outerScope;
            using (outerScope = this.tracer.BuildSpan("OuterSpan")
                .IgnoreActiveSpan()
                .WithTag("tag", "value")
                .WithStartTimestamp(DateTimeOffset.MaxValue)
                .StartActive(finishSpanOnDispose: true))
            {
            }

            CollectionAssert.AreEqual(
                new[]
                {
                    new SpanEvent(outerScope.Span, SpanEventType.Activated),
                    new SpanEvent(outerScope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());
        }

        [TestMethod]
        public void StartInactive()
        {
            IScope outerScope;
            IScope innerScope;

            var outerSpan = this.tracer.BuildSpan("OuterSpan").Start();
            var innerSpan = this.tracer.BuildSpan("InnerSpan").Start();

            using (outerScope = this.tracer.ScopeManager.Activate(outerSpan, finishSpanOnDispose: true))
            {
                using (innerScope = this.tracer.ScopeManager.Activate(innerSpan, finishSpanOnDispose: true))
                {
                    // Do nothing
                }
            }

            CollectionAssert.AreEqual(
                new[]
                {
                    new SpanEvent(outerScope.Span, SpanEventType.Activated),
                    new SpanEvent(innerScope.Span, SpanEventType.Activated),
                    new SpanEvent(innerScope.Span, SpanEventType.Finished),
                    new SpanEvent(outerScope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());
        }

        [TestMethod]
        public void CloseTheActiveSpan()
        {
            var scope = this.tracer.BuildSpan("Span").StartActive(finishSpanOnDispose: false);

            this.tracer.ActiveSpan.Finish();

            CollectionAssert.AreEqual(
                new[]
                {
                    new SpanEvent(scope.Span, SpanEventType.Activated),
                    new SpanEvent(scope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());
        }

        [TestMethod]
        public void CloseTheActiveScopeFromScopeManager()
        {
            var scope = this.tracer.BuildSpan("Span").StartActive(finishSpanOnDispose: true);

            this.tracer.ScopeManager.Active.Dispose();

            CollectionAssert.AreEqual(
                new[]
                {
                    new SpanEvent(scope.Span, SpanEventType.Activated),
                    new SpanEvent(scope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());
        }

        [TestMethod]
        public void CloseTheActiveSpanFromScopeManager()
        {
            var scope = this.tracer.BuildSpan("Span").StartActive(finishSpanOnDispose: false);

            this.tracer.ScopeManager.Active.Span.Finish();

            CollectionAssert.AreEqual(
                new[]
                {
                    new SpanEvent(scope.Span, SpanEventType.Activated),
                    new SpanEvent(scope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());
        }

        private enum SpanEventType
        {
            Activated,
            Finished
        }

        private sealed class SpanEvent
        {
            public SpanEventType Type { get; }
            
            public ISpan Span { get; }

            public SpanEvent(ISpan span, SpanEventType type)
            {
                this.Type = type;
                this.Span = span;
            }
        }

        private sealed class SpanEventComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var xSpanEvent = x as SpanEvent;
                var ySpanEvent = y as SpanEvent;

                ISpan xSpan = xSpanEvent.Span;
                ISpan ySpan = ySpanEvent.Span;

                if (xSpan is EventHookSpan eXSpan)
                {
                    xSpan = eXSpan._spanImplementation;
                }

                if (ySpan is EventHookSpan eYSpan)
                {
                    ySpan = eYSpan._spanImplementation;
                }

                if (!ReferenceEquals(xSpan, ySpan))
                {
                    return -1;
                }

                if (xSpanEvent.Type != ySpanEvent.Type)
                {
                    return -1;
                }

                return 0;
            }
        }
    }
}
