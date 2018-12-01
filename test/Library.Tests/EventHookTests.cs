using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Library.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using OpenTracing;
    using OpenTracing.Contrib.EventHookTracer;
    using OpenTracing.Mock;

    [TestClass]
    public class BaggageTests{
        
        private EventHookTracer tracer;

        [TestInitialize]
        public void Initialize()
        {
            var eventTracer= new EventHookTracer();
            this.tracer = eventTracer;
        }

        [TestMethod]
        public void BaggageOnSameItem()
        {
            var scope = this.tracer.BuildSpan("1")
                .StartActive();
            scope.Span.SetBaggageItem("key", "value");

            Assert.AreEqual("value", scope.Span.GetBaggageItem("key"));
        }

        [TestMethod]
        public void BaggagePropagatesToChildren()
        {
            using (var scope = this.tracer.BuildSpan("1")
                .StartActive())
            {
                scope.Span.SetBaggageItem("key", "value");

                using (var child = this.tracer.BuildSpan("2")
                    .StartActive())
                {
                    Assert.AreEqual("value", child.Span.GetBaggageItem("key"));
                }
            }
        }
        

        [TestMethod]
        public void ChildDoesNotAffectParent()
        {
            var scope = this.tracer.BuildSpan("1")
                .StartActive();

            var child = this.tracer.BuildSpan("2")
                .StartActive();
            child.Span.SetBaggageItem("key", "value");

            Assert.AreEqual(null, scope.Span.GetBaggageItem("key"));
        }
    }

    [TestClass]
    public class EventHookTests
    {
        private ITracer tracer;
        private IList<SpanEvent> events;
        private IList<Tuple<ISpan, LogEventArgs>> logEvents;
        private IList<Tuple<ISpan, SetTagEventArgs>> setTagEvents;

        [TestInitialize]
        public void Initialize()
        {
            events = new List<SpanEvent>();
            this.logEvents = new List<Tuple<ISpan, LogEventArgs>>();
            this.setTagEvents = new List<Tuple<ISpan, SetTagEventArgs>>();

            var eventTracer= new EventHookTracer();
            eventTracer.SpanActivated += (sender, span) => { this.events.Add(new SpanEvent(span.Span, SpanEventType.Activated)); };
            eventTracer.SpanActivating += (sender, span) => { this.events.Add(new SpanEvent(span.Span, SpanEventType.Activating)); };
            eventTracer.SpanFinished += (sender, span) => { this.events.Add(new SpanEvent(span.Span, SpanEventType.Finished)); };
            eventTracer.SpanFinishing += (sender, span) => { this.events.Add(new SpanEvent(span.Span, SpanEventType.Finishing)); };
            eventTracer.SpanLog += (sender, args) => { this.logEvents.Add(Tuple.Create((ISpan)sender, args)); };
            eventTracer.SpanSetTag += (sender, args) => { this.setTagEvents.Add(Tuple.Create((ISpan)sender, args)); };

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
                    innerScope.Span.SetTag("test", "testValue");
                }

                outerScope.Span.Log("log event");
                Thread.Sleep(10);
                outerScope.Span.Log("log event2");
            }

            CollectionAssert.AreEqual(
                new[]
                {
                    new SpanEvent(outerScope.Span, SpanEventType.Activating),
                    new SpanEvent(outerScope.Span, SpanEventType.Activated),

                    new SpanEvent(innerScope.Span, SpanEventType.Activating),
                    new SpanEvent(innerScope.Span, SpanEventType.Activated),

                    new SpanEvent(innerScope.Span, SpanEventType.Finishing),
                    new SpanEvent(innerScope.Span, SpanEventType.Finished),

                    new SpanEvent(outerScope.Span, SpanEventType.Finishing),
                    new SpanEvent(outerScope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());

            var singleTag = this.setTagEvents.Single();
            Assert.AreEqual("test", singleTag.Item2.Key);
            Assert.AreEqual("testValue", singleTag.Item2.Value);

            var firstLog = this.logEvents.First();
            var firstField = firstLog.Item2.Fields.Single();
            Assert.AreEqual("event", firstField.Key);
            Assert.AreEqual("log event", firstField.Value);
            
            var secondLog = this.logEvents.ElementAt(1);
            var secondField = secondLog.Item2.Fields.Single();
            Assert.AreEqual("event", secondField.Key);
            Assert.AreEqual("log event2", secondField.Value);

            Assert.IsTrue(secondLog.Item2.Timestamp > firstLog.Item2.Timestamp);
        }

        /// <summary>
        /// Validates not only the call order of Activate/Finish, but also that the state between these operations is as expected
        /// </summary>
        [TestMethod]
        public void ProperOrderingOfEvents()
        {
            EventHookSpan activeSpanWhenActivating = null;
            EventHookSpan activeSpanWhenActivated = null;
            EventHookSpan activeSpanWhenFinishing = null;
            EventHookSpan activeSpanWhenFinished = null;

            var eventHookTracer = new EventHookTracer();
            eventHookTracer.SpanActivating += (sender, args) =>
            {
                activeSpanWhenActivating = eventHookTracer.ActiveSpan;
            };
            eventHookTracer.SpanActivated += (sender, args) =>
            {
                activeSpanWhenActivated = eventHookTracer.ActiveSpan;
            };
            eventHookTracer.SpanFinishing += (sender, args) =>
            {
                activeSpanWhenFinishing = eventHookTracer.ActiveSpan;
            };
            eventHookTracer.SpanFinished += (sender, args) =>
            {
                activeSpanWhenFinished = eventHookTracer.ActiveSpan;
            };

            EventHookSpan testSpan;
            using (var testScope = eventHookTracer.BuildSpan("TestSpan").StartActive(finishSpanOnDispose: true))
            {
                testSpan = testScope.Span;
            }

            Assert.AreEqual(null, activeSpanWhenActivating);
            Assert.AreNotEqual(null, activeSpanWhenActivated);
            Assert.AreNotEqual(null, activeSpanWhenFinishing);
            Assert.AreEqual(null, activeSpanWhenFinished);

            Assert.AreEqual(activeSpanWhenActivated, activeSpanWhenFinishing);
            Assert.AreEqual(testSpan, activeSpanWhenActivated);
        }

        [TestMethod]
        public void WithSomeModifications() // TODO: What kind of name is that?
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
                    new SpanEvent(outerScope.Span, SpanEventType.Activating),
                    new SpanEvent(outerScope.Span, SpanEventType.Activated),
                    new SpanEvent(outerScope.Span, SpanEventType.Finishing),
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
                    new SpanEvent(outerScope.Span, SpanEventType.Activating),
                    new SpanEvent(outerScope.Span, SpanEventType.Activated),

                    new SpanEvent(innerScope.Span, SpanEventType.Activating),
                    new SpanEvent(innerScope.Span, SpanEventType.Activated),

                    new SpanEvent(innerScope.Span, SpanEventType.Finishing),
                    new SpanEvent(innerScope.Span, SpanEventType.Finished),

                    new SpanEvent(outerScope.Span, SpanEventType.Finishing),
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
                    new SpanEvent(scope.Span, SpanEventType.Activating),
                    new SpanEvent(scope.Span, SpanEventType.Activated),
                    new SpanEvent(scope.Span, SpanEventType.Finishing),
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
                    new SpanEvent(scope.Span, SpanEventType.Activating),
                    new SpanEvent(scope.Span, SpanEventType.Activated),
                    new SpanEvent(scope.Span, SpanEventType.Finishing),
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
                    new SpanEvent(scope.Span, SpanEventType.Activating),
                    new SpanEvent(scope.Span, SpanEventType.Activated),
                    new SpanEvent(scope.Span, SpanEventType.Finishing),
                    new SpanEvent(scope.Span, SpanEventType.Finished),
                },
                this.events.ToList(),
                new SpanEventComparer());
        }

        private enum SpanEventType
        {
            Activated,
            Activating,
            Finished,
            Finishing,
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

                if (xSpanEvent.Type != ySpanEvent.Type)
                {
                    return -1;
                }

                if (xSpan is EventHookSpan eXSpan
                    && ySpan is EventHookSpan eYSpan)
                {
                    return ReferenceEquals(eXSpan.onActivated, eYSpan.onActivated) ? 0 : -1;
                }

                return -1;
            }
        }
    }
}
