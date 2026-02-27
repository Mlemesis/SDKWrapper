using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CentralTech.CTResilientAnalytics;
using CentralTech.CTEventSystem;
using Object = UnityEngine.Object;

public class ResiliantAnalyticsSystemTests
{
    private ResilientAnalyticsSystem _analyticsSystem;
    private MockEventSystem _mockEventSystem;
    private float _waitTime = 6f; //needs to be long enough to allow for any stalled processes

    [SetUp]
    public void Setup()
    {
        _mockEventSystem = new MockEventSystem();
        _analyticsSystem = new ResilientAnalyticsSystem(_mockEventSystem, queueMaximumTime: 5f);
    }

    [TearDown]
    public void Teardown()
    {
        if (_analyticsSystem != null)
        {
            _analyticsSystem.Destroy();
        }
    }

    [UnityTest]
    public IEnumerator SendEvent_WithBlockedSending_DoesNotProcessQueue()
    {
        // Arrange
        _analyticsSystem.BlockEventSending();
        _mockEventSystem.TriggeredEvents.Clear();

        // Act
        _analyticsSystem.SendEvent("TestEvent");
        yield return new WaitForSeconds(_waitTime);

        // Assert
        // Event should be queued but not processed yet
        Assert.That(_mockEventSystem.TriggeredEvents.Count, Is.EqualTo(0));
    }

    [UnityTest]
    public IEnumerator SendEvent_ProcessesQueueAfterUnblocking()
    {
        // Arrange
        _analyticsSystem.BlockEventSending();
        _analyticsSystem.SendEvent("TestEvent1");
        _analyticsSystem.SendEvent("TestEvent2");
        // Act
        _analyticsSystem.UnblockEventSending();
        
        // Wait for coroutine to process
        yield return new WaitForSeconds(_waitTime);
        
        // Assert
        Assert.That(_mockEventSystem.TriggeredEvents.Count, Is.GreaterThan(0), 
            "Events should be processed after unblocking");
        var sentEvent = _mockEventSystem.TriggeredEvents[0] as AnalyticSentEvent?;
        Assert.That(sentEvent?.EventName, Is.EqualTo("TestEvent1"));
    }

    [UnityTest]
    public IEnumerator SendEvent_TriggersAnalyticSentEvent()
    {
        // Act
        _analyticsSystem.SendEvent("MyEvent");
        
        // Wait for event to be processed
        yield return new WaitForSeconds(_waitTime);

        // Assert
        Assert.That(_mockEventSystem.TriggeredEvents.Count, Is.GreaterThan(0));
        var analyticEvent = _mockEventSystem.TriggeredEvents[0] as AnalyticSentEvent?;
        Assert.That(analyticEvent?.EventName, Is.EqualTo("MyEvent"));
    }

    [UnityTest]
    public IEnumerator BlockEventSending_PreventsQueueProcessing()
    {
        // Arrange
        _analyticsSystem.BlockEventSending();
        _mockEventSystem.TriggeredEvents.Clear();

        // Act
        _analyticsSystem.SendEvent("BlockedEvent");
        yield return new WaitForSeconds(_waitTime);

        // Assert - Event is queued but not immediately processed
        Assert.That(_mockEventSystem.TriggeredEvents.Count, Is.EqualTo(0));
    }

    [UnityTest]
    public IEnumerator ProcessQueue_StopsAfterMaximumTime()
    {
        // Arrange - Use a very short queue maximum time
        var shortTimeSystem = new ResilientAnalyticsSystem(_mockEventSystem, queueMaximumTime: 0.01f);
        var eventsBeforeDestroy = _mockEventSystem.TriggeredEvents.Count;
        
        // Act - Send multiple events
        for (int i = 0; i < 5; i++)
        {
            shortTimeSystem.SendEvent($"Event{i}");
        }

        yield return new WaitForSeconds(_waitTime);

        // Assert - Some events should be processed
        var eventsAfter = _mockEventSystem.TriggeredEvents.Count;
        Assert.That(eventsAfter, Is.GreaterThan(eventsBeforeDestroy), 
            "At least some events should be processed");

        // Cleanup
        shortTimeSystem.Destroy();
    }

    [UnityTest]
    public IEnumerator Destroy_CleansUpCoroutineRunner()
    {
        // Arrange
        _analyticsSystem.SendEvent("Event");
        var coroutineGameObject = Object.FindObjectOfType<CoroutineRunner>();
        Assert.That(coroutineGameObject, Is.Not.Null, "CoroutineRunner should exist");

        // Act
        _analyticsSystem.Destroy();
        yield return new WaitForSeconds(_waitTime);
        // Assert
        var coroutineAfterDestroy = Object.FindObjectOfType<CoroutineRunner>();
        Assert.That(coroutineAfterDestroy, Is.Null, "CoroutineRunner should be destroyed");
    }

    [Test]
    public void Interface_ReturnsCorrectType()
    {
        // Assert
        Assert.That(_analyticsSystem.Interface, Is.EqualTo(typeof(IResilientAnalyticsSystem)));
    }
}

/// <summary>
/// Mock implementation of IEventSystem for testing
/// </summary>
public class MockEventSystem : IEventSystem
{
    public List<IEvent> TriggeredEvents { get; } = new List<IEvent>();

    public void RegisterEvent<T>(EventSystem.EventCallBack eventCallBack) where T : IEvent
    {
        // Not needed for these tests
    }

    public void UnregisterEvent<T>(EventSystem.EventCallBack eventCallBack) where T : IEvent
    {
        // Not needed for these tests
    }

    public void TriggerEvent(IEvent eventObject)
    {
        TriggeredEvents.Add(eventObject);
    }

    public Type Interface => typeof(IEventSystem);

    public void Destroy()
    {
        TriggeredEvents.Clear();
    }
}
