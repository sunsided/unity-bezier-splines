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
            InspectorSubdivisions();
            InspectorNodeList();

            // base.OnInspectorGUI();
            _path = (BezierPath) target;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() => _gui.DrawAllNodes((BezierPath) target);

        [NotNull]
        private static List<BezierPathNode> GetNodeList([NotNull] Transform parent) => parent.GetComponentsInChildren<BezierPathNode>().ToList();

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

            EditorGUI.BeginChangeCheck();
            var newDrawMode = (BezierPath.GizmoDrawMode) EditorGUILayout.EnumPopup("Gizmo Draw Mode", (BezierPath.GizmoDrawMode) gizmoMode.enumValueIndex);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Gizmo Mode");
                gizmoMode.enumValueIndex = (int) newDrawMode;
            }
        }

        private void InspectorSubdivisions()
        {
            var subdivisions = serializedObject.FindProperty("subdivisions");
            Debug.Assert(subdivisions != null, "subdivisions != null");

            var previousValue = subdivisions.intValue;
            if (previousValue < 0)
            {
                previousValue = 1;
            }

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.IntField("Subdivisions", previousValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Subdivisions");

                if (newValue < 0)
                {
                    newValue = 1;
                }

                subdivisions.intValue = newValue;
            }
        }

        private void InspectorNodeList()
        {
            _showWaypoints = EditorGUILayout.BeginFoldoutHeaderGroup(_showWaypoints, new GUIContent("Waypoints"), null, InspectorNodeListContextMenu);
            if (!_showWaypoints) return;

            var nodes = GetNodeList(((BezierPath) target).transform);

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
            var nodes = GetNodeList(((BezierPath) target).transform);

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
            var nodes = GetNodeList(((BezierPath) target).transform);

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