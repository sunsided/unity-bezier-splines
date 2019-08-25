using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezier
{
    public class BezierPath : MonoBehaviour
    {
        [SerializeField]
        private GizmoDrawMode gizmoDrawMode = GizmoDrawMode.Complete;

        [SerializeField]
        private bool closed;

        [SerializeField, NotNull]
        private List<BezierPathNode> nodes = new List<BezierPathNode>();

        public bool Closed => closed;

        private void OnDrawGizmos()
        {
            // Only draw Gizmos when the object is unselected.
            // If the object is selected, the inspector takes care of it.
            if (Selection.activeObject == gameObject) return;

            var drawMode = gizmoDrawMode;
            if (drawMode == GizmoDrawMode.None) return;

            var tf = transform;
            Gizmos.matrix = tf.localToWorldMatrix;

            var waypointColor = Color.magenta;
            var waypointConnectionColor = waypointColor;

            for (var index = 0; index < nodes.Count; ++index)
            {
                var node = nodes[index];
                if (node == null) continue;
                 var pos = node.Position;

                // Waypoint
                Gizmos.color = waypointColor;
                Gizmos.DrawCube(pos, Vector3.one * 0.125f);
                if (drawMode == GizmoDrawMode.WaypointOnly) continue;

                // Line connecting to previous waypoint
                if (index <= 0) continue;
                var previous = nodes[index - 1];

                Gizmos.color = waypointConnectionColor;
                Gizmos.DrawLine(previous.Position, pos);
            }

            // Line connecting to previous waypoint
            if (!closed) return;
            if (drawMode != GizmoDrawMode.Complete || nodes.Count <= 2) return;
            var first = nodes[0];
            var last = nodes[nodes.Count - 1];

            Gizmos.color = waypointConnectionColor;
            Gizmos.DrawLine(last.Position, first.Position);
        }

        [Serializable]
        public enum GizmoDrawMode
        {
            Complete,
            WaypointOnly,
            None
        }
    }
}
