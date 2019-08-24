using System;
using UnityEngine;

namespace Playground
{
    [ExecuteInEditMode]
    public class HandleBasedLineSegments : MonoBehaviour
    {
        public bool closed;
        public Node[] nodes;

        [Serializable]
        public class Node
        {
            public Vector3 position;
            public Quaternion rotation;
        }
    }
}
