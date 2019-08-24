using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezier.Editor
{
    [CustomEditor(typeof(BezierPath))]
    public class BezierPathEditor : UnityEditor.Editor
    {
        private const int LineWidth = 3;

        private BezierPath _path;

        private bool _showWaypoints = true;

        private void OnEnable()
        {
            _path = (BezierPath) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorClosedPath();
            InspectorGizmoDrawMode();
            InspectorNodeList();

            // base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var closedPath = serializedObject.FindProperty("closed").boolValue;
            var nodesProp = serializedObject.FindProperty("nodes");
            var nodeCount = nodesProp.arraySize;

            var size = HandleUtility.GetHandleSize(_path.transform.position);
            var snap = Vector3.up * 0.1f;

            var pathTransform = _path.transform;
            // Handles.Label(pathTransform.position, "start");

            // Draw individual node segments.
            for (var index = 0; index < nodeCount; ++index)
            {
                var node = _path[index]; // TODO: Do this via serialized property
                var connected = node.type == BezierPath.NodeType.Connected;

                // Make sure Gizmos/Handle coordinates are local to the parent.
                Handles.matrix = pathTransform.localToWorldMatrix;

                // Connect waypoints
                if (index > 0)
                {
                    Handles.color = Color.grey;
                    Handles.DrawDottedLine(_path[index - 1].position, node.position, LineWidth);
                }

                // Draw position handle
                // TODO: We may want to allow a rotation control here anyways.
                EditorGUI.BeginChangeCheck();
                Handles.color = Color.gray;
                var nodePosition = Handles.FreeMoveHandle(node.position, Quaternion.identity, size * 0.125f, snap, Handles.CubeHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Position of waypoint");
                    node.position = nodePosition;
                }

                // Connect current outgoing control with next incoming control.
                if (index < nodeCount - 1)
                {
                    var next = _path[index + 1];

                    Handles.color = Color.magenta;
                    Handles.DrawLine(
                        node.Out + nodePosition,
                        next.In + next.position);
                }

                // Make sure Gizmos/Handle coordinates are local to the parent.
                Handles.matrix *= Matrix4x4.Translate(nodePosition);

                // Draw incoming control handle. Unconnected paths don't need an initial incoming control.
                if (index > 0 || closedPath)
                {
                    Handles.color = Color.blue;
                    Handles.DrawLine(Vector3.zero, node.In);

                    EditorGUI.BeginChangeCheck();
                    var handlePos = Handles.FreeMoveHandle(node.In, Quaternion.identity, size * 0.125f, snap,
                        Handles.SphereHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Position of control");
                        node.In = handlePos;
                    }
                }

                // Draw outgoing control handle. Unconnected paths don't need a final outgoing control.
                if (index < nodeCount - 1 || closedPath)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(Vector3.zero, node.Out);

                    EditorGUI.BeginChangeCheck();
                    var handlePos = Handles.FreeMoveHandle(node.Out, Quaternion.identity, size * 0.125f, snap,
                        Handles.SphereHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Position of control");
                        node.Out = handlePos;
                    }
                }
            }

            // Connect waypoints
            if (closedPath)
            {
                Handles.matrix = pathTransform.localToWorldMatrix;

                var first = _path[0];
                var last = _path[nodeCount - 1];

                Handles.color = Color.grey;
                Handles.DrawDottedLine(last.position, first.position, LineWidth);

                // Connect in- and out-points.
                Handles.color = Color.magenta;
                Handles.DrawLine(
                    last.Out + last.position,
                    first.In + first.position);
            }
        }

        private void InspectorClosedPath()
        {
            var nodeType = serializedObject.FindProperty("closed");
            var newValue = EditorGUILayout.Toggle("Closed Path", nodeType.boolValue);
            nodeType.boolValue = newValue;
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
            _showWaypoints = EditorGUILayout.BeginFoldoutHeaderGroup(_showWaypoints, new GUIContent("Waypoints"), null, InspectorNodeListContextMenu);
            if (!_showWaypoints) return;

            var nodes = GetNodesProperty();

            for (var i = 0; i < nodes.arraySize; ++i)
            {
                var element = nodes.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, true);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void InspectorNodeListContextMenu(Rect position)
        {
            var nodes = GetNodesProperty();

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Append waypoint"), false, OnAddWaypointToEndItemClicked);
            menu.AddItem(new GUIContent("Prepend waypoint"), false, OnAddWaypointToStartItemClicked );

            if (nodes.arraySize > 0)
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Remove all waypoints"), false, OnRemoveAllNodesItemClicked);
            }

            menu.DropDown(position);
        }

        private void OnAddWaypointToStartItemClicked()
        {
            var nodes = GetNodesProperty();

            // Increasing the array/list size will duplicate the last item, or
            // add a default-initialized item to the end.
            nodes.InsertArrayElementAtIndex(0);

            // To reflect our change, we need to serialize the list.
            nodes.serializedObject.ApplyModifiedProperties();

            // Lastly, since the constructor wasn't involved, we're now going
            // to actively initialize the node.
            _path.First().Reset(); // TODO: Do this via serialized property
        }

        private void OnAddWaypointToEndItemClicked()
        {
            var nodes = GetNodesProperty();

            // Increasing the array/list size will duplicate the last item, or
            // add a default-initialized item to the end.
            nodes.arraySize++;

            // To reflect our change, we need to serialize the list.
            nodes.serializedObject.ApplyModifiedProperties();

            // Lastly, since the constructor wasn't involved, we're now going
            // to actively initialize the node.
            _path.Last().Reset(); // TODO: Do this via serialized property
        }

        private void OnRemoveAllNodesItemClicked()
        {
            var nodes = GetNodesProperty();

            var shouldRemove = EditorUtility.DisplayDialog(
                "Removing waypoints",
                $"Are you sure you want to remove {nodes.arraySize} waypoints from the bath?", "Remove", "Cancel");
            if (!shouldRemove) return;

            nodes.ClearArray();
            Debug.Log("All nodes were removed from the path.");

            nodes.serializedObject.ApplyModifiedProperties();
        }

        [NotNull]
        private SerializedProperty GetNodesProperty()
        {
            var nodes = serializedObject.FindProperty("nodes");
            Debug.Assert(nodes != null, "nodes != null");
            Debug.Assert(nodes.isArray, "nodes.isArray == true");
            return nodes;
        }
    }
}