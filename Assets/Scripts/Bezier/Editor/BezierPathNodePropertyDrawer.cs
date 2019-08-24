using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezier.Editor
{
    [CustomPropertyDrawer(typeof(BezierPath.Node))]
    public class BezierPathNodePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, property);

            if (!IsValid(property))
            {
                EditorGUILayout.HelpBox("Uninitialized node.", MessageType.Error);
            }
            else
            {
                EditorGUI.LabelField(new Rect(pos.x, pos.y, pos.width, pos.height), label.text, EditorStyles.boldLabel);

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;

                InspectorGizmoDrawMode(property);
                InspectorWaypointPosition(property);
                InspectorInPosition(property);
                InspectorOutPosition(property);

                EditorGUI.indentLevel = indent;
            }

            EditorGUI.EndProperty();
        }

        private void InspectorGizmoDrawMode([NotNull] SerializedProperty property)
        {
            var nodeType = GetNodeTypeProperty(property);
            var posPropIn = property.FindPropertyRelative("_in");
            var posPropOut = property.FindPropertyRelative("_out");

            var newNodeType = (BezierPath.NodeType) EditorGUILayout.EnumPopup("Node Type",
                (BezierPath.NodeType) nodeType.enumValueIndex);

            var valueChanged = nodeType.enumValueIndex != (int) newNodeType;
            nodeType.enumValueIndex = (int) newNodeType;

            if (valueChanged && newNodeType == BezierPath.NodeType.Connected)
            {
                // TODO: Fix this duplication of node logic!
                if (posPropIn.vector3Value != -posPropOut.vector3Value)
                {
                    posPropOut.vector3Value = -posPropIn.vector3Value;
                }
            }
        }

        private static void InspectorWaypointPosition(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            var posProp = property.FindPropertyRelative(nameof(BezierPath.Node.position));
            var newValue = EditorGUILayout.Vector3Field(new GUIContent("Position"), posProp.vector3Value);
            if (EditorGUI.EndChangeCheck())
            {
                posProp.vector3Value = newValue;
            }
        }

        private static void InspectorInPosition(SerializedProperty property)
        {
            var posPropIn = property.FindPropertyRelative("_in");
            var posPropOut = property.FindPropertyRelative("_out");
            var type = (BezierPath.NodeType)property.FindPropertyRelative("type").enumValueIndex;

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Vector3Field(new GUIContent("Ingoing Control"), posPropIn.vector3Value);
            if (EditorGUI.EndChangeCheck())
            {
                // TODO: Fix this duplication of node logic!
                posPropIn.vector3Value = newValue;
                if (type == BezierPath.NodeType.Connected) posPropOut.vector3Value = -newValue;
            }
        }

        private static void InspectorOutPosition(SerializedProperty property)
        {
            var posPropIn = property.FindPropertyRelative("_in");
            var posPropOut = property.FindPropertyRelative("_out");
            var type = (BezierPath.NodeType) property.FindPropertyRelative("type").enumValueIndex;

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Vector3Field(new GUIContent("Outgoing Control"), posPropOut.vector3Value);
            if (EditorGUI.EndChangeCheck())
            {
                // TODO: Fix this duplication of node logic!
                posPropOut.vector3Value = newValue;
                if (type == BezierPath.NodeType.Connected) posPropIn.vector3Value = -newValue;
            }
        }

        private bool IsValid([NotNull] SerializedProperty property) => null != property.FindPropertyRelative(nameof(BezierPath.Node.type));

        [NotNull]
        private static SerializedProperty GetNodeTypeProperty([NotNull] SerializedProperty property)
        {
            var nodeType = property.FindPropertyRelative(nameof(BezierPath.Node.type));
            Debug.Assert(nodeType != null, "nodeType != null");
            return nodeType;
        }
    }
}