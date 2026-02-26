using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CentralTech.SystemsBase;
using Interview.Mocks;
using UnityEngine;

namespace CentralTech.CTResilientAnalytics
{
    public interface IResilientAnalyticsSystemV2 : IGenericSystem
    {
        void SendEvent(string eventName);
        void Update();
    }
    
    public class ResilientAnalyticsSystemV2 : IResilientAnalyticsSystemV2
    {
        private UnstableLegacyService _legacyService;
        private CancellationTokenSource _masterCancellationTokenSource;
        private List<CancellationTokenSource> _activeTasks = new();
        private List<string> _eventQueue = new();
        
        private float _queueProcessInterval = 5f; // Process queue every 5 seconds
        private float _timeSinceLastProcess = 0f;
        
        public ResilientAnalyticsSystemV2(float queueProcessInterval)
        {
            _legacyService = new UnstableLegacyService();
            _masterCancellationTokenSource = new CancellationTokenSource();
            _queueProcessInterval = queueProcessInterval;
        }
        
        public void SendEvent(string eventName)
        {
            CancellationTokenSource taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_masterCancellationTokenSource.Token);
            _activeTasks.Add(taskCancellationTokenSource);
            
            Task.Run(() => SendEventsAsync(eventName, taskCancellationTokenSource.Token), taskCancellationTokenSource.Token)
                .ContinueWith(_ => 
                {
                    _activeTasks.Remove(taskCancellationTokenSource);
                    taskCancellationTokenSource.Dispose();
                }, TaskScheduler.Default);
        }

        private Task SendEventsAsync(string eventName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            bool success = false;
            try
            {
                success = _legacyService.SendEvent(eventName);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Sending event failed: " + e.Message + "Will cache and try again");
            }
            
            if (!success)
            {
                _eventQueue.Add(eventName);
            }

            return Task.CompletedTask;
        }

        public void Update()
        {
            _timeSinceLastProcess += Time.deltaTime;

            if (_timeSinceLastProcess >= _queueProcessInterval && _eventQueue.Count > 0)
            {
                _timeSinceLastProcess = 0f;
                ProcessEventQueue();
            }
        }

        private void ProcessEventQueue()
        {
            List<string> eventsToRetry = new List<string>(_eventQueue);
            _eventQueue.Clear();

            foreach (var eventName in eventsToRetry)
            {
                SendEvent(eventName);
            }
        }
        
        public Type Interface => typeof(IResilientAnalyticsSystemV2);
 
        public void Destroy()
        {
            if (_masterCancellationTokenSource != null && !_masterCancellationTokenSource.IsCancellationRequested)
            {
                _masterCancellationTokenSource.Cancel();
                _masterCancellationTokenSource.Dispose();
            }

            foreach (var taskTokenSource in _activeTasks)
            {
                if (!taskTokenSource.IsCancellationRequested)
                {
                    taskTokenSource.Cancel();
                }
                taskTokenSource.Dispose();
            }
            
            _activeTasks.Clear();
        }
    }
}