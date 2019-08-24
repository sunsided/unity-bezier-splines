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
            var newNodeType = (BezierPath.NodeType) EditorGUILayout.EnumPopup("Node Type",
                (BezierPath.NodeType) nodeType.enumValueIndex);
            nodeType.enumValueIndex = (int) newNodeType;
        }

        private static void InspectorPositionControl([NotNull] SerializedProperty property, [NotNull] string name, [NotNull] GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var posProp = property.FindPropertyRelative(name);
            var newValue = EditorGUILayout.Vector3Field(label, posProp.vector3Value);
            if (EditorGUI.EndChangeCheck())
            {
                posProp.vector3Value = newValue;
            }
        }

        private static void InspectorWaypointPosition(SerializedProperty property) =>
            InspectorPositionControl(property, nameof(BezierPath.Node.position), new GUIContent("Position"));

        private static void InspectorInPosition(SerializedProperty property) =>
            InspectorPositionControl(property, nameof(BezierPath.Node.@in), new GUIContent("Ingoing Control"));

        private static void InspectorOutPosition(SerializedProperty property) =>
            InspectorPositionControl(property, nameof(BezierPath.Node.@out), new GUIContent("Outgoing Control"));

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