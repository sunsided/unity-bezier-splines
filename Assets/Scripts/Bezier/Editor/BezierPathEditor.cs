using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezier.Editor
{
    [CustomEditor(typeof(BezierPath))]
    public class BezierPathEditor : UnityEditor.Editor
    {
        private BezierPath _path;

        private bool _showWaypoints = true;

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
            _path.First().Reset();
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
            _path.Last().Reset();
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