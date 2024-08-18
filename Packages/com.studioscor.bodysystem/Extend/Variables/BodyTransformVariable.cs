using StudioScor.Utilities;
using System;
using UnityEngine;

namespace StudioScor.BodySystem.Extend.Variable
{
    [Serializable]
    public class BodyTransformVariable : TransformVariable
    {
        [Header(" [ Body Transform Variable ] ")]
        [SerializeField] private BodyTag _bodyTag;

        private IBodySystem _bodySystem;
        private BodyTransformVariable _original;

        public override void Setup(GameObject owner)
        {
            base.Setup(owner);

            _bodySystem = owner.GetBodySystem();
        }

        public override ITransformVariable Clone()
        {
            var clone = new BodyTransformVariable();

            clone._original = this;

            return clone;
        }

        public override Transform GetValue()
        {
            var tag = _original is null ? _bodyTag : _original._bodyTag;

            if (_bodySystem.TryGetBodyPart(tag, out IBodyPart bodypart))
            {
                return bodypart.transform;
            }
            else
            {
                return _bodySystem.transform;
            }
        }
    }
}
