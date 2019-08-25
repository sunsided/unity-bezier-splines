using System;
using UnityEngine;

namespace Bezier
{
    [ExecuteInEditMode]
    public class BezierPathNode : MonoBehaviour
    {
        [SerializeField]
        private NodeType type;

        [SerializeField]
        private Vector3 @in;

        [SerializeField]
        private Vector3 @out;

        private Vector3 _previousIn;
        private Vector3 _previousOut;

        public NodeType Type
        {
            get => type;
            set => type = value;
        }

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Vector3 In
        {
            get => @in;
            set
            {
                @in = value;
                UpdateFromIncoming();
            }
        }

        public Vector3 Out
        {
            get => @out;
            set
            {
                @out = value;
                UpdateFromOutgoing();
            }
        }

        public void UpdateFromIncoming()
        {
            ref var value = ref @in;
            if (type == NodeType.Connected)
            {
                @out = -value.normalized * @out.magnitude;
                TakeControlSnapshot();
            }
            else if (type == NodeType.Symmetric)
            {
                @out = -value;
                TakeControlSnapshot();
            }
        }

        public void UpdateFromOutgoing()
        {
            ref var value = ref @out;
            if (type == NodeType.Connected)
            {
                @in = -value.normalized * @in.magnitude;
                TakeControlSnapshot();
            }
            else if (type == NodeType.Symmetric)
            {
                @in = -value;
                TakeControlSnapshot();
            }
        }

        public void Reset()
        {
            type = NodeType.Connected;
            @in = new Vector3(0f, 0, -.5f);
            @out = new Vector3(0f, 0, .5f);
        }

        private void Awake()
        {
            TakeControlSnapshot();
        }

        private void Update()
        {
            if (_previousIn != @in)
            {
                UpdateFromIncoming();
            }
            else if (_previousOut != @out)
            {
                UpdateFromOutgoing();
            }

            TakeControlSnapshot();
        }

        private void TakeControlSnapshot()
        {
            _previousIn = @in;
            _previousOut = @out;
        }

        [Serializable]
        public enum NodeType
        {
            Connected,
            Symmetric,
            Broken
        }

        public enum Direction
        {
            Incoming,
            Outgoing,
        }
    }
}