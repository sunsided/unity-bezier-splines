using System;
using System.Collections.Generic;
using System.Linq;
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
        private int subdivisions = 30;

        [SerializeField]
        private bool closed;

        public bool Closed => closed;

        public int Subdivisions => subdivisions;

        /// <summary>
        /// Determines whether a child of the path is currently selected in the editor.
        /// </summary>
        private bool ChildSelectedInEditor => Selection.activeGameObject != null &&
                                              Selection.activeGameObject.transform.parent != null &&
                                              Selection.activeGameObject.transform.parent.gameObject == gameObject;

        /// <summary>
        /// Determines whether this path is currently selected in the editor.
        /// </summary>
        private bool SelectedInEditor => Selection.activeGameObject == gameObject;

        /// <summary>
        /// Calculates the point at distance <paramref name="t"/> along a cubic B-spline defined by four points.
        /// </summary>
        /// <param name="p1">The starting point.</param>
        /// <param name="p2">The first support point.</param>
        /// <param name="p3">The second support point.</param>
        /// <param name="p4">The end point.</param>
        /// <param name="t">The distance along the curve.</param>
        /// <returns>The calculated point along the curve.</returns>
        public static Vector3 CalculatePoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
        {
            Debug.Assert(p1 != p2, "p1 != p2");
            Debug.Assert(p2 != p3, "p2 != p3");
            Debug.Assert(p3 != p4, "p3 != p4");
            // TODO: When the pairs (p1,p2), (p2,p3) or (p3,p4) are on the same point, we have a quadratic B-spline.
            // TODO: When the pairs (p1,p2,p3) or (p2,p3,p4) are on the same point, we have a line.

            // TODO: This isn't really needed - we can extrapolate points if we need to.
            t = Mathf.Clamp01(t);

            var it = 1 - t;
            var it2 = it * it;
            var it3 = it2 * it;

            var t2 = t * t;
            var t3 = t2 * t;

            var part1 = it3 * p1;
            var part2 = 3 * t * it2 * p2;
            var part3 = 3 * t2 * it * p3;
            var part4 = t3 * p4;

            return part1 + part2 + part3 + part4;
        }

        private static float Pow3(float t) => t * t * t;

        [NotNull]
        private static List<BezierPathNode> GetNodeList([NotNull] Transform parent) =>
            parent.GetComponentsInChildren<BezierPathNode>().ToList();

        private void OnDrawGizmos()
        {
            var tf = transform;
            var nodes = GetNodeList(tf);

            DrawSplineGizmos(nodes);
            DrawSupportGizmos(nodes);
        }

        private void DrawSplineGizmos<T>([NotNull] T nodes)
            where T : IReadOnlyList<BezierPathNode>
        {
            var shouldRender = gizmoDrawMode != GizmoDrawMode.Complete && gizmoDrawMode != GizmoDrawMode.SplineOnly;
            if (shouldRender) return;

            var splineColor = Color.gray;
            Gizmos.color = splineColor;

            var invSubdivisions = 1f / subdivisions;
            var iterateThrough = Closed ? nodes.Count : nodes.Count - 1;
            for (var i = 0; i < iterateThrough; ++i)
            {
                var nextIndex = i + 1;
                if (nextIndex >= nodes.Count) nextIndex = 0;

                var current = nodes[i];
                var next = nodes[nextIndex];

                var p0 = current.Center;
                var p1 = current.Out + current.Center;
                var p2 = next.In + next.Center;
                var p3 = next.Center;

                var previousPoint = p0;
                for (var step = 1; step <= subdivisions; ++step)
                {
                    var t = step * invSubdivisions;
                    var point = CalculatePoint(p0, p1, p2, p3, t);

                    Gizmos.DrawLine(previousPoint, point);
                    previousPoint = point;
                }
            }
        }

        private void DrawSupportGizmos<T>([NotNull] T nodes)
            where T : IReadOnlyList<BezierPathNode>
        {
            // Only draw Gizmos when the object is unselected.
            // If the object is selected, the inspector takes care of it.
            if (SelectedInEditor || ChildSelectedInEditor) return;

            var shouldRender = gizmoDrawMode == GizmoDrawMode.None || gizmoDrawMode == GizmoDrawMode.SplineOnly;
            if (shouldRender) return;

            var waypointColor = Color.magenta;
            for (var index = 0; index < nodes.Count; ++index)
            {
                var node = nodes[index];
                Debug.Assert(node != null, "node != null");
                var pos = node.Center;

                // Waypoint
                Gizmos.color = waypointColor;
                Gizmos.DrawCube(pos, Vector3.one * 0.125f);
            }
        }

        [Serializable]
        public enum GizmoDrawMode
        {
            Complete,
            SplineOnly,
            WaypointOnly,
            None
        }
    }
}
