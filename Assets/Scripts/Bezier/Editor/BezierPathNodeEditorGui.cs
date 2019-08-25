using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezier.Editor
{
    public class BezierPathNodeEditorGui
    {
        private const int DottedLineWidth = 1;
        private const int DashedLineWidth = 3;

        /// <summary>
        ///     Draws the Inspector GUI for a specific node.
        /// </summary>
        /// <param name="node">The serialized node.</param>
        /// <param name="positionProperty">
        ///     The serialized <c>transform.position</c> property of the node.
        ///     If <see langword="null"/>, the position property will not be rendered.
        /// </param>
        public void DrawNodeInspectorGui(
            [NotNull] SerializedObject node,
            [CanBeNull] SerializedProperty positionProperty)
        {
            var typeProp = node.FindProperty("type");
            var inPosProp = node.FindProperty("in");
            var outPosProp = node.FindProperty("out");

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Label
            var label = new GUIContent("Waypoint");
            if (!node.isEditingMultipleObjects)
            {
                var gameObject = (BezierPathNode) node.targetObject;
                var siblingIndex = ((BezierPathNode) node.targetObject).transform.GetSiblingIndex();
                label = new GUIContent($"Waypoint #{siblingIndex + 1} ({gameObject.name})");
            }

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Node type
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Node Type"), true);

            // Transform
            if (positionProperty != null)
            {
                EditorGUILayout.PropertyField(positionProperty, new GUIContent("Position"), true);
            }

            // Incoming Control
            EditorGUILayout.PropertyField(inPosProp, new GUIContent("Incoming Control"));

            // Outgoing Control
            EditorGUILayout.PropertyField(outPosProp, new GUIContent("Outgoing Control"));

            EditorGUI.indentLevel = indent;
        }

        /// <summary>
        /// Draws the Scene GUI for a specific node.
        /// </summary>
        /// <param name="parentPath">The transform of the owning <see cref="BezierPath"/>.</param>
        /// <param name="node">The node to render.</param>
        /// <param name="isActive">Whether the node to be rendered is considered to be active, i.e. by selection.</param>
        /// <param name="drawIncoming">Whether the incoming connection should be rendered, e.g. for the first node.</param>
        /// <param name="drawOutgoing">Whether the outgoing connection should be rendered, e.g. for the last node.</param>
        public void DrawNodeSceneGui([NotNull] Transform parentPath, [NotNull] BezierPathNode node, bool isActive, bool drawIncoming, bool drawOutgoing)
        {
            using (new Handles.DrawingScope(Handles.matrix))
            {
                var transform = node.transform;
                var size = HandleUtility.GetHandleSize(transform.position);
                var snap = Vector3.up * 0.1f;

                var positionCap = isActive ? (Handles.CapFunction) Handles.CubeHandleCap : Handles.RectangleHandleCap;
                var controlCap = isActive ? (Handles.CapFunction) Handles.SphereHandleCap : Handles.CircleHandleCap;

                var positionCapSize = size * (isActive ? 0.125f : 0.0625f);
                var controlCapSize = size * (isActive ? 0.125f : 0.0625f);

                // Handles.matrix = parentPath.localToWorldMatrix;

                // Draw position handle
                // TODO: We may want to allow a rotation control here anyways.
                using (new Handles.DrawingScope(Color.gray))
                {
                    EditorGUI.BeginChangeCheck();
                    var nodePosition = Handles.FreeMoveHandle(node.Position, Quaternion.identity, positionCapSize, snap,
                        positionCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(node, "Position of waypoint");
                        node.Position = nodePosition;
                        // TODO: Selection.activeGameObject = node.gameObject;
                    }
                }

                // Make sure Gizmos/Handle coordinates are local to the parent.
                using (new Handles.DrawingScope(Handles.matrix * transform.localToWorldMatrix))
                {
                    // Draw incoming control handle. Unconnected paths don't need an initial incoming control.
                    if (drawIncoming)
                    {
                        using (new Handles.DrawingScope(Color.blue))
                        {
                            Handles.DrawLine(Vector3.zero, node.In);

                            EditorGUI.BeginChangeCheck();
                            var handlePos = Handles.FreeMoveHandle(node.In, Quaternion.identity, controlCapSize, snap,
                                controlCap);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(node, "Position of control");
                                node.In = handlePos;
                                // TODO: Selection.activeGameObject = node.gameObject;
                            }
                        }
                    }

                    // Draw outgoing control handle. Unconnected paths don't need a final outgoing control.
                    if (drawOutgoing)
                    {
                        using (new Handles.DrawingScope(Color.red))
                        {
                            Handles.DrawLine(Vector3.zero, node.Out);

                            EditorGUI.BeginChangeCheck();
                            var handlePos = Handles.FreeMoveHandle(node.Out, Quaternion.identity, controlCapSize, snap,
                                controlCap);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(node, "Position of control");
                                node.Out = handlePos;
                                // TODO: Selection.activeGameObject = node.gameObject;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Draws all nodes that are siblings of the specified <paramref name="node"/>.
        /// </summary>
        /// <param name="node">The node whose siblings to render.</param>
        public void DrawAllNodesFrom([NotNull] BezierPathNode node)
        {
            var path = node.transform.parent.GetComponent<BezierPath>();
            if (path == null) return;
            DrawAllNodes(path, node);
        }

        /// <summary>
        ///     Draw all nodes in a specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to render child nodes from.</param>
        /// <param name="selectedNode">
        ///     The node that is considered to be active (e.g., selected) in the specified path;
        ///     if <see langword="null"/>, no node is highlighted.
        /// </param>
        public void DrawAllNodes([NotNull] BezierPath path, [CanBeNull] BezierPathNode selectedNode = null)
        {
            var transform = path.transform;
            var closedPath = path.Closed;

            var nodes = GetNodeList(transform);

            var lastNodeIndex = nodes.Count - 1;
            for (var i = 0; i < nodes.Count; ++i)
            {
                var currentNode = nodes[i];
                var nextNode = DetermineNextNode(i);

                var drawIncoming = i > 0 || closedPath;
                var drawOutgoing = i < lastNodeIndex || closedPath;
                var isActive = currentNode == selectedNode;

                // Make sure Gizmos/Handle coordinates are local to the parent.
                using (new Handles.DrawingScope(Handles.matrix))
                {
                    // Connect waypoints
                    var shouldDrawConnection = i < lastNodeIndex || (nodes.Count > 2);
                    if (nextNode != null && shouldDrawConnection)
                    {
                        using (new Handles.DrawingScope(Color.gray))
                        {
                            Handles.DrawDottedLine(currentNode.Position, nextNode.Position, DashedLineWidth);
                        }
                    }

                    // Connect current outgoing control with next incoming control.
                    if (nextNode != null)
                    {
                        using (new Handles.DrawingScope(Color.magenta))
                        {
                            Handles.DrawDottedLine(
                                currentNode.Out + currentNode.Position,
                                nextNode.In + nextNode.Position,
                                DottedLineWidth);
                        }
                    }

                    // Draw the actual handles.
                    DrawNodeSceneGui(transform, currentNode, isActive, drawIncoming, drawOutgoing);
                }
            }

            BezierPathNode DetermineNextNode(int i) =>
                i < lastNodeIndex
                    ? nodes[i + 1]
                    : closedPath
                        ? nodes[0]
                        : null;
        }

        /// <summary>
        /// Gets all child nodes in a parent transform, where the parent is assumed to be a <see cref="BezierPath"/>.
        /// </summary>
        /// <param name="parent">The parent transform.</param>
        /// <returns>The list of child nodes.</returns>
        [NotNull]
        private static List<BezierPathNode> GetNodeList([NotNull] Transform parent) =>
            parent.GetComponentsInChildren<BezierPathNode>().ToList();
    }
}