using StudioScor.Utilities;
using System;
using UnityEngine;

namespace StudioScor.BodySystem.Extend.Variable
{
    [Serializable]
    public class HasBodyConditionVariable : ConditionVariable
    {
        [Header(" [ Has Body Condition Variable ] ")]
        [SerializeField] private BodyTag _bodyTag;

        private IBodySystem _bodySystem;
        private HasBodyConditionVariable _original;

        public override void Setup(GameObject owner)
        {
            base.Setup(owner);

            _bodySystem = owner.GetBodySystem();
        }

        public override IVariable<bool> Clone()
        {
            var clone = new HasBodyConditionVariable();

            clone._original = this;

            return clone;
        }

        public override bool GetValue()
        {
            var tag = _original is null ? _bodyTag : _original._bodyTag;

            return _bodySystem.HasBodyPart(tag);
        }
    }
}
