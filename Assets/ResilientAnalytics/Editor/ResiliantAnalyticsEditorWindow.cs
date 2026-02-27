using System;
using UnityEngine;
using UnityEditor;
using CentralTech.CTEditorTools.Editor;
using CentralTech.CTEventSystem;
using CentralTech.CTSystemsBase;

namespace CentralTech.CTResilientAnalytics.Editor
{
    public class ResiliantAnalyticsEditorWindow : EditorWindow
    {
        private IEditorLayoutHelper _editorLayoutHelper;
        private IEventSystem _eventSystem;
        private ResilientAnalyticsSystem _resilientAnalyticsSystem;
        private bool _setupForPlayMode = false;

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
        }

        private void OnGUI()
        {
            SetupVariables();
        }
        
        private void OnAnalyticSent(IEvent eventObject)
        {
            
        }

        private void SetupVariables()
        {
            if (_editorLayoutHelper == null)
            {
                _editorLayoutHelper = new EditorLayoutHelper();
            }
            if (!_setupForPlayMode && Application.isPlaying)
            {
                _eventSystem = SystemProvider.Instance.GetSystem<IEventSystem>();
                _resilientAnalyticsSystem = SystemProvider.Instance.GetSystem<ResilientAnalyticsSystem>();
                RegisterToEvent();
                _setupForPlayMode = true;
            }
            else if (_setupForPlayMode && !Application.isPlaying)
            {
                _setupForPlayMode = false;
                _eventSystem = new EventSystem();
                RegisterToEvent();
                _resilientAnalyticsSystem = new ResilientAnalyticsSystem(_eventSystem);
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
}
