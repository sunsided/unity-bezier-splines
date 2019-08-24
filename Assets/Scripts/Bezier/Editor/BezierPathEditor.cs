using System;
using UnityEditor;
using UnityEngine;

namespace Bezier.Editor
{
    [CustomEditor(typeof(BezierPath))]
    public class BezierPathEditor : UnityEditor.Editor
    {
        private BezierPath _path;

        private void OnEnable()
        {
            _path = (BezierPath) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorGizmoDrawMode();
            InspectorNodeList();

            // base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
        }

        private void InspectorGizmoDrawMode()
        {
            var gizmoMode = serializedObject.FindProperty("gizmoDrawMode");
            Debug.Assert(gizmoMode != null, "gizmoMode != null");
            var newDrawMode = (BezierPath.GizmoDrawMode) EditorGUILayout.EnumPopup("Gizmo Draw Mode", (BezierPath.GizmoDrawMode) gizmoMode.enumValueIndex);
            gizmoMode.enumValueIndex = (int) newDrawMode;
        }

        private void InspectorNodeList()
        {
            var nodes = serializedObject.FindProperty("nodes");
            Debug.Assert(nodes != null, "nodes != null");

            for (var i = 0; i < nodes.arraySize; ++i)
            {
                var element = nodes.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, true);
            }
        }
    }
}