using FutharksAdventure.Core;
using NUnit.Framework;
using System;

namespace FutharksAdventure.Tests
{
    public class EventBusTests
    {
        private struct TestEvent { public int Value; }
        private struct OtherEvent { }

        private EventBus _bus;

        [SetUp]
        public void SetUp() => _bus = new EventBus();

        [Test]
        public void Publish_CallsSubscriberWithEvent()
        {
            int received = 0;
            _bus.Subscribe<TestEvent>(e => received = e.Value);

            _bus.Publish(new TestEvent { Value = 42 });

            Assert.That(received, Is.EqualTo(42));
        }

        [Test]
        public void Publish_CallsAllSubscribers()
        {
            int calls = 0;
            _bus.Subscribe<TestEvent>(_ => calls++);
            _bus.Subscribe<TestEvent>(_ => calls++);

            _bus.Publish(new TestEvent());

            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void Publish_WithoutSubscriber_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent()));
        }

        [Test]
        public void Publish_DoesNotCallSubscribersOfOtherType()
        {
            bool called = false;
            _bus.Subscribe<OtherEvent>(_ => called = true);

            _bus.Publish(new TestEvent());

            Assert.That(called, Is.False);
        }

        [Test]
        public void Unsubscribe_StopsReceivingEvents()
        {
            int calls = 0;
            void Handler(TestEvent e) => calls++;
            _bus.Subscribe<TestEvent>(Handler);
            _bus.Unsubscribe<TestEvent>(Handler);

            _bus.Publish(new TestEvent());

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Subscribe_SameHandlerTwice_CallsItOnce()
        {
            int calls = 0;
            void Handler(TestEvent e) => calls++;
            _bus.Subscribe<TestEvent>(Handler);
            _bus.Subscribe<TestEvent>(Handler);

            _bus.Publish(new TestEvent());

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Subscribe_SameHandlerTwice_ThenUnsubscribeOnce_StopsReceiving()
        {
            int calls = 0;
            void Handler(TestEvent e) => calls++;
            _bus.Subscribe<TestEvent>(Handler);
            _bus.Subscribe<TestEvent>(Handler);
            _bus.Unsubscribe<TestEvent>(Handler);

            _bus.Publish(new TestEvent());

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Subscribe_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Subscribe<TestEvent>(null));
        }

        [Test]
        public void Unsubscribe_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Unsubscribe<TestEvent>(null));
        }

        [Test]
        public void Unsubscribe_TypeNeverSubscribed_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bus.Unsubscribe<OtherEvent>(_ => { }));
        }

        [Test]
        public void Unsubscribe_UnknownHandler_KeepsOtherSubscribers()
        {
            int calls = 0;
            _bus.Subscribe<TestEvent>(_ => calls++);

            _bus.Unsubscribe<TestEvent>(_ => { });
            _bus.Publish(new TestEvent());

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Unsubscribe_DuringPublish_DoesNotThrowAndStopsNextPublish()
        {
            int calls = 0;
            void Handler(TestEvent e) { calls++; _bus.Unsubscribe<TestEvent>(Handler); }
            _bus.Subscribe<TestEvent>(Handler);

            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent()));
            _bus.Publish(new TestEvent());

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Subscribe_DuringPublish_NewHandlerCalledFromNextPublishOnly()
        {
            int lateCalls = 0;
            void Late(TestEvent e) => lateCalls++;
            void Early(TestEvent e) => _bus.Subscribe<TestEvent>(Late);
            _bus.Subscribe<TestEvent>(Early);

            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent()));
            Assert.That(lateCalls, Is.EqualTo(0));

            _bus.Publish(new TestEvent());
            Assert.That(lateCalls, Is.EqualTo(1));
        }
    }
}