using UnityEngine;
using UnityEditor;
using StudioScor.Utilities.Editor;

namespace StudioScor.StateMachine.Editor
{
    [CustomEditor(typeof(TransformContainer))]
    [CanEditMultipleObjects]
    public class TransformContainerEditor : DictionaryContainerEditor<StateMachineComponent, Transform>
    {
        public override string KeyName(StateMachineComponent key)
        {
            return key ? key.gameObject.name : "NULL";
        }

        public override string ValueName(Transform value)
        {
            return value ? value.gameObject.name : "NULL";
        }
    }
}
