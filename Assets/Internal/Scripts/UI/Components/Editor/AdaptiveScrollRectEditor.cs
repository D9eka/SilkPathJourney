using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Components.Editor
{
    [CustomEditor(typeof(AdaptiveScrollRect))]
    public class AdaptiveScrollRectEditor : ScrollRectEditor
    {
        private SerializedProperty _scrollbarPadding;

        protected override void OnEnable()
        {
            base.OnEnable();
            _scrollbarPadding = serializedObject.FindProperty("_scrollbarPadding");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var vVisibility = serializedObject.FindProperty("m_VerticalScrollbarVisibility");
            if (vVisibility != null && vVisibility.intValue != (int)ScrollRect.ScrollbarVisibility.Permanent)
            {
                vVisibility.intValue = (int)ScrollRect.ScrollbarVisibility.Permanent;
                serializedObject.ApplyModifiedProperties();
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(_scrollbarPadding);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
