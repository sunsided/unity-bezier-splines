using UnityEditor;
using UnityEngine;

namespace Playground.Editor
{
    [CustomEditor(typeof(HandleBasedLineSegments))]
    public class HandleBasedLineSegmentEditor : UnityEditor.Editor
    {
        private const int LineWidth = 3;

        private HandleBasedLineSegments _lineSegments;
        private HandleBasedLineSegments.Node[] _nodes;

        private void OnEnable()
        {
            _lineSegments = (HandleBasedLineSegments) target;
            _nodes = _lineSegments.nodes;
        }

        public override void OnInspectorGUI() => DrawDefaultInspector();

        private void OnSceneGUI()
        {
            // TODO: Use FreeMoveHandle for control points

            Handles.Label(_lineSegments.transform.position, "YEAH");

            // Make sure Gizmos/Handle coordinates are local to the parent.
            Handles.matrix = _lineSegments.transform.localToWorldMatrix;

            // Draw individual node segments.
            for (var index = 0; index < _nodes.Length; ++index)
            {
                ref var node = ref _nodes[index];

                // Draw position handle
                var showMove = Tools.current == Tool.Move || Tools.current == Tool.Transform;
                if (showMove)
                {
                    EditorGUI.BeginChangeCheck();
                    var newPos = Handles.PositionHandle(node.position, node.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Position of handle");
                        node.position = newPos;
                    }
                }

                // Draw rotation handle
                var showRotation = Tools.current == Tool.Rotate || Tools.current == Tool.Transform;
                if (showRotation)
                {
                    EditorGUI.BeginChangeCheck();
                    var newRot = Handles.RotationHandle(node.rotation, node.position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotation of handle");
                        node.rotation = newRot;
                    }
                }

                // Connecting line
                if (index > 0)
                {
                    Handles.DrawDottedLine(_nodes[index - 1].position, node.position, LineWidth);
                }
            }

            // Add line connection between first and last node
            if (_lineSegments.closed && _nodes.Length > 2)
            {
                Handles.DrawDottedLine(_nodes[_nodes.Length - 1].position, _nodes[0].position, LineWidth);
            }

            if (Event.current.type == EventType.Repaint)
            {
                for (var index = 0; index < _nodes.Length; ++index)
                {
                    ref var node = ref _nodes[index];
                    var size = HandleUtility.GetHandleSize(node.position) * 0.125f;
                    Handles.CircleHandleCap(index, node.position, Quaternion.identity, size, EventType.Repaint);
                }
            }
        }
    }
}