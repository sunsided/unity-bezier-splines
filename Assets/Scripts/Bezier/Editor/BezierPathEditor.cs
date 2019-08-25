using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezier.Editor
{
    [CustomEditor(typeof(BezierPath))]
    public class BezierPathEditor : UnityEditor.Editor
    {
        [NotNull]
        private readonly BezierPathNodeEditorGui _gui = new BezierPathNodeEditorGui();

        private BezierPath _path;

        private bool _showWaypoints = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorClosedPath();
            InspectorGizmoDrawMode();
            InspectorNodeList();

            // base.OnInspectorGUI();
            _path = (BezierPath) target;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() => _gui.DrawAllNodes((BezierPath) target);

        [NotNull]
        private static List<BezierPathNode> GetNodesList([NotNull] Transform parent) => parent.GetComponentsInChildren<BezierPathNode>().ToList();

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

            var nodes = GetNodesList(((BezierPath) target).transform);

            for (var i = 0; i < nodes.Count; ++i)
            {
                var node = nodes[i];
                var element = new SerializedObject(node);
                var transform = new SerializedObject(node.gameObject.transform);
                var posProp = transform.FindProperty("m_LocalPosition");

                _gui.DrawNodeInspectorGui(element, posProp);

                transform.ApplyModifiedProperties();
                element.ApplyModifiedProperties();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void InspectorNodeListContextMenu(Rect position)
        {
            var nodes = GetNodesList(((BezierPath) target).transform);

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add waypoint to start"), false, OnAddWaypointToEndItemClicked);
            menu.AddItem(new GUIContent("Add waypoint to end"), false, OnAddWaypointToStartItemClicked);

            if (nodes.Count > 0)
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Remove all waypoints"), false, OnRemoveAllNodesItemClicked);
            }

            menu.DropDown(position);
        }

        private void OnAddWaypointToStartItemClicked()
        {
            // Lastly, since the constructor wasn't involved, we're now going
            // to actively initialize the node.
            // TODO: Do this to all selected paths?
            var newNode = new GameObject(nameof(BezierPathNode), typeof(BezierPathNode));

            var tf = newNode.transform;
            tf.parent = _path.transform;
            tf.SetAsFirstSibling();

            // _path.First().Reset(); // TODO: Do this via serialized property
        }

        private void OnAddWaypointToEndItemClicked()
        {
            // Lastly, since the constructor wasn't involved, we're now going
            // to actively initialize the node.
            // TODO: Do this to all selected paths?
            var newNode = new GameObject(nameof(BezierPathNode), typeof(BezierPathNode));

            var tf = newNode.transform;
            tf.parent = _path.transform;
            tf.SetAsLastSibling();
        }

        private void OnRemoveAllNodesItemClicked()
        {
            var nodes = GetNodesList(((BezierPath) target).transform);

            var shouldRemove = EditorUtility.DisplayDialog(
                "Removing waypoints",
                $"Are you sure you want to remove {nodes.Count} waypoints from the bath?", "Remove", "Cancel");
            if (!shouldRemove) return;

            foreach (var node in nodes)
            {
                DestroyImmediate(node);
            }
            Debug.Log("All nodes were removed from the path.");
        }
    }
}