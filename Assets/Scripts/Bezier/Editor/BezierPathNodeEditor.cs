using JetBrains.Annotations;
using UnityEditor;

namespace Bezier.Editor
{
    [CustomEditor(typeof(BezierPathNode))]
    [CanEditMultipleObjects]
    public class BezierPathNodeEditor : UnityEditor.Editor
    {
        [NotNull]
        private readonly BezierPathNodeEditorGui _gui = new BezierPathNodeEditorGui();

        private BezierPathNode _node;

        private void Awake()
        {
            _node = (BezierPathNode) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _gui.DrawNodeInspectorGui(serializedObject, null);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() => _gui.DrawAllNodesFrom(_node);
    }
}