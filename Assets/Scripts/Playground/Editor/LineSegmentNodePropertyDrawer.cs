using UnityEditor;
using UnityEngine;

namespace Playground.Editor
{
    [CustomPropertyDrawer(typeof(GizmoBasedLineSegments.Node))]
    public class LineSegmentNodePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            var posProp = property.FindPropertyRelative(nameof(GizmoBasedLineSegments.Node.position));

            EditorGUI.BeginProperty(pos, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.Vector3Field(
                new Rect (pos.x, pos.y, pos.width, pos.height),
                label.text, posProp.vector3Value);
            if (EditorGUI.EndChangeCheck())
            {
                posProp.vector3Value = newValue;
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}