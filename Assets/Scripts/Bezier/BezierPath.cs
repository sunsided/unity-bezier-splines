using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezier
{
    public class BezierPath : MonoBehaviour, IEnumerable<BezierPath.Node>
    {
        [SerializeField]
        private GizmoDrawMode gizmoDrawMode = GizmoDrawMode.Complete;

        [SerializeField]
        private bool closed;

        [SerializeField, NotNull]
        private List<Node> nodes = new List<Node>();

        public int Length => nodes.Count;

        [NotNull]
        public Node this[int index] => nodes[index];

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
            if (!closed) return;
            if (drawMode != GizmoDrawMode.Complete || nodes.Count <= 2) return;
            var first = nodes[0];
            var last = nodes[nodes.Count - 1];

            Gizmos.color = waypointConnectionColor;
            Gizmos.DrawLine(last.position, first.position);
        }

        [Serializable]
        public class Node
        {
            public NodeType type;
            public Vector3 position;

            [SerializeField]
            private Vector3 _in;

            [SerializeField]
            private Vector3 _out;

            public Node() => Reset();

            public Vector3 In
            {
                get => _in;
                set
                {
                    _in = value;
                    if (type != NodeType.Connected) return;
                    _out = -value;
                }
            }

            public Vector3 Out
            {
                get => _out;
                set
                {
                    _out = value;
                    if (type != NodeType.Connected) return;
                    _in = -value;
                }
            }

            public void Reset()
            {
                type = NodeType.Connected;
                position = new Vector3(0f, 0, 0);
                _in = new Vector3(0f, 0, -.5f);
                _out = new Vector3(0f, 0, .5f);
            }

            public override string ToString() => $"{type} at: {position}, in: {_in}, out: {_out}";
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

        public IEnumerator<Node> GetEnumerator() => nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) nodes).GetEnumerator();
    }
}
