using System;
using UnityEngine;

namespace Playground
{
    [ExecuteInEditMode]
    public class GizmoBasedLineSegments : MonoBehaviour
    {
        public bool closed;
        public Node[] nodes;

        private void OnDrawGizmos()
        {
            var tf = transform;
            var pos = tf.position * 0;

            Gizmos.color = Color.magenta;
            Gizmos.matrix = tf.localToWorldMatrix;

            for (var index = 0; index < nodes.Length; ++index)
            {
                var node = nodes[index];
                if (node == null) continue;

                Gizmos.DrawSphere(node.position + pos, 0.125f);

                if (index > 0)
                {
                    Gizmos.DrawLine(nodes[index - 1].position + pos, node.position + pos);
                }
            }

            // Close the loop if requested.
            if (closed && nodes.Length > 2)
            {
                Gizmos.DrawLine(nodes[nodes.Length - 1].position + pos, nodes[0].position + pos);
            }
        }

        [Serializable]
        public class Node
        {
            public Vector3 position;
        }
    }
}
