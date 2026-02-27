using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CentralTech.CTEventSystem;
using CentralTech.CTSystemsBase;

namespace CentralTech.CTResilientAnalytics.Editor
{
    public class ResiliantAnalyticsEditorWindow : EditorWindow
    {
        private IEventSystem _eventSystem;
        private IResilientAnalyticsSystem _resilientAnalyticsSystem;

        // Cache for analytic events
        private List<AnalyticSentEvent> _analyticEventCache = new();
        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _statisticsScrollPosition = Vector2.zero;
        private string _eventNameInput = "";
        
        // Track success/failure/retries per event name
        private Dictionary<string, EventStatistics> _eventStats = new();

        [MenuItem("CT Tools/Resilient Analytics Monitor")]
        public static void ShowWindow()
        {
            ResiliantAnalyticsEditorWindow window =
                GetWindow<ResiliantAnalyticsEditorWindow>("Resilient Analytics Monitor");
            window.minSize = new Vector2(500, 300);
        }

        private void OnDestroy()
        {
            UnregisterFromEvent();
            _resilientAnalyticsSystem?.Destroy();
        }

        private void OnGUI()
        {
            SetupVariables();
            if (_resilientAnalyticsSystem == null)
            {
                EditorGUILayout.HelpBox("Enter play mode to use monitor", MessageType.Error);
                return;
            }
            DrawAnalyticsUI();
        }

        private void DrawAnalyticsUI()
        {
            EditorGUILayout.LabelField("Analytic Events Monitor", EditorStyles.boldLabel);

            // Draw status banner at the top
            DrawStatusBanner();

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear Cache", GUILayout.Height(30)))
            {
                _analyticEventCache.Clear();
                _eventStats.Clear();
            }

            EditorGUILayout.LabelField($"Total Events: {_analyticEventCache.Count}", EditorStyles.helpBox);

            // Draw statistics summary with scroll
            EditorGUILayout.LabelField("Event Statistics", EditorStyles.boldLabel);

            if (_eventStats.Count == 0)
            {
                EditorGUILayout.HelpBox("No event statistics available yet", MessageType.Info);
            }
            else
            {
                _statisticsScrollPosition =
                    EditorGUILayout.BeginScrollView(_statisticsScrollPosition, GUILayout.Height(350));

                foreach (var kvp in _eventStats)
                {
                    string eventName = kvp.Key;
                    EventStatistics stats = kvp.Value;

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Event: {eventName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  Successes: {stats.SuccessCount}", EditorStyles.label);
                    EditorGUILayout.LabelField($"  Failures: {stats.FailureCount}", EditorStyles.label);
                    EditorGUILayout.LabelField($"  Retries: {stats.RetryCount}", EditorStyles.label);
                    EditorGUILayout.LabelField($"  Total Attempts: {stats.TotalAttempts}", EditorStyles.label);
                    EditorGUILayout.LabelField($"  Avg Time: {stats.GetAverageTime():F3}s", EditorStyles.label);
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
      
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Event Details", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(350));

            for (int i = 0; i < _analyticEventCache.Count; i++)
            {
                DrawAnalyticEventEntry(_analyticEventCache[i], i);
            }

            EditorGUILayout.EndScrollView();
        }


        private void DrawStatusBanner()
        {
            int totalSuccesses = 0;
            int totalFailures = 0;

            foreach (var stats in _eventStats.Values)
            {
                totalSuccesses += stats.SuccessCount;
                totalFailures += stats.FailureCount;
            }

            string status = GetCircuitBreakerStatus(totalSuccesses, totalFailures);
            Color statusColor = GetStatusColor(status);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            Color originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField($"Status: {status}", EditorStyles.boldLabel);
            GUI.color = originalColor;
            
            string successRate = GetSuccessRate(totalSuccesses, totalFailures).ToString("F1");
            EditorGUILayout.LabelField($"Total Successes: {totalSuccesses} | Total Failures: {totalFailures} | Success Rate: {successRate}%", EditorStyles.label);
            
            EditorGUILayout.EndVertical();
        }

        private string GetCircuitBreakerStatus(int successes, int failures)
        {
            int total = successes + failures;
            if (failures == 0  || total == 0)
            {
                return "🟢 Closed (All systems operational)";
            }
            
            float failureRate = (float)failures / total;
            
            if (failureRate > 0.5f)
            {
                return "🔴 Open (Too many failures)";
            }
            if (failureRate > 0.2f)
            {
                return "🟡 Half-Open (Some failures detected)";
            }
  
            return "🟢 Closed (All systems operational)";
            
        }

        private Color GetStatusColor(string status)
        {
            if (status.Contains("Closed"))
            {
                return new Color(0.3f, 0.8f, 0.3f); // Green
            }
            if (status.Contains("Open"))
            {
                return new Color(0.9f, 0.3f, 0.3f); // Red
            }
            return new Color(1f, 0.8f, 0.3f); // Yellow
            
        }

        private float GetSuccessRate(int successes, int failures)
        {
            if (successes + failures == 0)
            {
                return 0f;
            }
            return (successes / (float)(successes + failures)) * 100f;
        }

        private void DrawAnalyticEventEntry(AnalyticSentEvent analyticEvent, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Event #{index + 1}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Event Name: {analyticEvent.EventName}");
            EditorGUILayout.LabelField($"Time Taken: {analyticEvent.TimeTaken:F3}s");
            
            string successText = analyticEvent.Success ? "✓ Success" : "✗ Failed";
            EditorGUILayout.LabelField($"Status: {successText}", EditorStyles.label);
            
            EditorGUILayout.LabelField($"Queue Size: {analyticEvent.QueueSize}");
            
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(analyticEvent.ErrorMessage))
            {
                EditorGUILayout.LabelField($"Error: {analyticEvent.ErrorMessage}", EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        private void OnAnalyticSent(IEvent eventObject)
        {
            if (eventObject is AnalyticSentEvent analyticEvent)
            {
                _analyticEventCache.Add(analyticEvent);
                UpdateEventStatistics(analyticEvent);
                Repaint();
            }
        }

        private void UpdateEventStatistics(AnalyticSentEvent analyticEvent)
        {
            if (!_eventStats.ContainsKey(analyticEvent.EventName))
            {
                _eventStats[analyticEvent.EventName] = new EventStatistics();
            }
            
            EventStatistics stats = _eventStats[analyticEvent.EventName];
            stats.TotalAttempts++;
            stats.TotalTimeTaken += analyticEvent.TimeTaken;
            
            if (analyticEvent.Success)
            {
                stats.SuccessCount++;
            }
            else
            {
                stats.FailureCount++;
                stats.RetryCount++;
            }
        }

        private void SetupVariables()
        {
            if (_resilientAnalyticsSystem == null && Application.isPlaying)
            {   
                _eventSystem = SystemProvider.Instance.GetSystem<IEventSystem>();
                _resilientAnalyticsSystem = SystemProvider.Instance.GetSystem<IResilientAnalyticsSystem>();
                
                RegisterToEvent();
            }
            else if (!Application.isPlaying)
            {
                _resilientAnalyticsSystem = null;
                _eventSystem = null;
            }
        }

        private void RegisterToEvent()
        {
            _eventSystem.RegisterEvent<AnalyticSentEvent>(OnAnalyticSent);
        }
        
        private void UnregisterFromEvent()
        {
            _eventSystem?.UnregisterEvent<AnalyticSentEvent>(OnAnalyticSent);
        }
    }

    /// <summary>
    /// Helper class to track statistics for individual events
    /// </summary>
    public class EventStatistics
    {
        public int SuccessCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        public int RetryCount { get; set; } = 0;
        public int TotalAttempts { get; set; } = 0;
        public float TotalTimeTaken { get; set; } = 0f;
        
        public float GetAverageTime()
        {
            if (TotalAttempts == 0)
                return 0f;
            return TotalTimeTaken / TotalAttempts;
        }
    }
}