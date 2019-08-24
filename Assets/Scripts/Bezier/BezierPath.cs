﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bezier
{
    public class BezierPath : MonoBehaviour
    {
        [SerializeField]
        private GizmoDrawMode gizmoDrawMode = GizmoDrawMode.Complete;

        [SerializeField]
        private List<Node> nodes = new List<Node>();

        private void OnDrawGizmos()
        {
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
                ref var pos = ref node.position;

                // Waypoint
                Gizmos.color = waypointColor;
                Gizmos.DrawCube(pos, Vector3.one * 0.125f);
                if (drawMode == GizmoDrawMode.WaypointOnly) continue;

                // Line connecting to previous waypoint
                if (index <= 0) continue;
                var previous = nodes[index - 1];

                Gizmos.color = waypointConnectionColor;
                Gizmos.DrawLine(previous.position, pos);
            }

            // Line connecting to previous waypoint
            if (drawMode != GizmoDrawMode.Complete || nodes.Count <= 2) return;
            var first = nodes[0];
            var last = nodes[nodes.Count - 1];

            Gizmos.color = waypointConnectionColor;
            Gizmos.DrawLine(last.position, first.position);
        }

        private void OnDrawGizmosSelected()
        {
            var drawMode = gizmoDrawMode;
            if (drawMode != GizmoDrawMode.Complete) return;

            var tf = transform;
            Gizmos.matrix = tf.localToWorldMatrix;

            var handleInColor = Color.blue;
            var handleOutColor = Color.red;
            var handleConnectionColor = Color.grey;

            for (var index = 0; index < nodes.Count; ++index)
            {
                var node = nodes[index];
                if (node == null) continue;
                ref var pos = ref node.position;

                // Handles
                Gizmos.color = handleInColor;
                Gizmos.DrawSphere(node.@in + pos, 0.125f * 0.5f);

                Gizmos.color = handleOutColor;
                Gizmos.DrawSphere(node.@out + pos, 0.125f * 0.5f);

                // Line connecting handles
                Gizmos.color = handleConnectionColor;
                Gizmos.DrawLine(node.@in + pos, node.@out + pos);
            }
        }

        [Serializable]
        public class Node
        {
            public NodeType type = NodeType.Connected;
            public Vector3 position = new Vector3(0f, 0, 0);
            public Vector3 @in = new Vector3(0f, 0, -1);
            public Vector3 @out = new Vector3(0f, 0, 1);
        }

        [Serializable]
        public enum NodeType
        {
            Connected,
            Broken
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
