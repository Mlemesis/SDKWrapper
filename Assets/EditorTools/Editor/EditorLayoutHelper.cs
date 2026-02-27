using UnityEditor;
using UnityEngine;

namespace CentralTech.CTEditorTools.Editor
{
    public interface IEditorLayoutHelper
    {
        GUIStyle HeaderStyle { get; }
        GUIStyle BoxStyle { get; }
        float DefaultButtonWidth { get; }
        Color Red { get; }
    }

//Note we could have a static helper here but I find its best to avoid statics with editor code and contain 
//code better in the window instances, other wise they can get out of sync with each other
    public class EditorLayoutHelper : IEditorLayoutHelper
    {
        private GUIStyle _headerStyle;
        private GUIStyle _boxStyle;
        private float _defaultButtonWidth = 80f;
        private Color _red = new Color(1f, 0.5f, 0.5f);

        public GUIStyle HeaderStyle => _headerStyle;
        public GUIStyle BoxStyle => _boxStyle;
        public float DefaultButtonWidth => _defaultButtonWidth;
        public Color Red => _red;

        public EditorLayoutHelper()
        {
            InitializeStyles();
        }

        private void InitializeStyles()
        {

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };

            _boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

        }
    }
}