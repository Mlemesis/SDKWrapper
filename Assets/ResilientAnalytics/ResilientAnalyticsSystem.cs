using System;
using System.Collections;
using System.Collections.Generic;
using CentralTech.CTEventSystem;
using CentralTech.CTSystemsBase;
using Interview.Mocks;
using UnityEngine;

namespace CentralTech.CTResilientAnalytics
{
    public struct AnalyticSentEvent : IEvent
    {
        public readonly string EventName;
        public readonly float TimeTaken;
        public readonly bool Success;
        public readonly string ErrorMessage;
        public readonly int QueueSize;

        public AnalyticSentEvent(string eventName, float timeTaken, bool success, string errorMessage, int queueSize)
        {
            EventName = eventName;
            TimeTaken = timeTaken;
            Success = success;
            ErrorMessage = errorMessage;
            QueueSize = queueSize;
        }
    }
    
    public interface IResilientAnalyticsSystem : IGenericSystem
    {
        void SendEvent(string eventName);
    }
    
    public class ResilientAnalyticsSystem : IResilientAnalyticsSystem
    {
        private UnstableLegacyService _legacyService;
        private List<string> _eventQueue = new();
        private float _queueMaximumTime = 5f;
        private CoroutineRunner _coroutineRunner;
        private Coroutine _processQueueCoroutine;
        private bool _isProcessingQueue = false;
        private IEventSystem _eventSystem;
        
        public ResilientAnalyticsSystem(IEventSystem eventSystem, float queueMaximumTime = 5f)
        {
            _legacyService = new UnstableLegacyService();
            _eventSystem = eventSystem;
            _queueMaximumTime = queueMaximumTime;
            
            // Create a GameObject to run coroutines on
            GameObject coroutineRunnerObject = new GameObject("ResilientAnalyticsCoroutineRunner");
            _coroutineRunner = coroutineRunnerObject.AddComponent<CoroutineRunner>();
        }
        
        public void SendEvent(string eventName)
        {
            _eventQueue.Add(eventName);
            if (!_isProcessingQueue)
            {
                _processQueueCoroutine = _coroutineRunner.StartCoroutine(ProcessQueueCoroutine());
            }
        }

        private (bool,string) SendEventWrapped(string eventName)
        {
            bool success = false;
            string errorMessage = "";
            try
            {
                success = _legacyService.SendEvent(eventName);
            }
            catch (Exception e)
            {
                errorMessage = "Sending event failed: " + e.Message + "Will cache and try again";
                Debug.LogWarning(errorMessage);
            }
            
            if (!success)
            {
                _eventQueue.Add(eventName);
            }
            return (success, errorMessage);
        }

        private IEnumerator ProcessQueueCoroutine()
        {
            _isProcessingQueue = true;
            float currentQueueTime = 0;
            while (_eventQueue.Count > 0)
            {
                string eventName = _eventQueue[0];
                _eventQueue.RemoveAt(0);
                
                float startTime = Time.time;
                (bool,string) success = SendEventWrapped(eventName);
                
                yield return null;
                
                float timeTaken = Time.time - startTime;
                
                _eventSystem.TriggerEvent(new AnalyticSentEvent(eventName, timeTaken, success.Item1, success.Item2, _eventQueue.Count));
                currentQueueTime += timeTaken;

                if (currentQueueTime > _queueMaximumTime)
                {
                    _coroutineRunner.StopCoroutine(_processQueueCoroutine);
                    _isProcessingQueue = false;
                }
            }
            _isProcessingQueue = false;
        }
        
        public Type Interface => typeof(IResilientAnalyticsSystem);
 
        public void Destroy()
        {
            if (_coroutineRunner != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    GameObject.DestroyImmediate(_coroutineRunner.gameObject);
                }
                else
                {
                    GameObject.Destroy(_coroutineRunner.gameObject);
                }
                
            }
        }
    }

    /// <summary>
    /// Helper MonoBehaviour to run coroutines for the analytics system
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        // This class exists solely to run coroutines
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}