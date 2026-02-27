using System;
using System.Collections;
using System.Collections.Generic;
using CentralTech.SystemsBase;
using Interview.Mocks;
using UnityEngine;

namespace CentralTech.CTResilientAnalytics
{
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
        
        public ResilientAnalyticsSystem(float queueMaximumTime = 5f)
        {
            _legacyService = new UnstableLegacyService();
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

        private void SendEventWrapped(string eventName)
        {
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
                SendEventWrapped(eventName);
                
                yield return null;
                
                float timeTaken = Time.time - startTime;
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
                GameObject.Destroy(_coroutineRunner.gameObject);
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