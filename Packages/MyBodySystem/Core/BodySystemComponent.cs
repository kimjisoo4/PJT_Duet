using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.BodySystem
{
    public interface IBodySystem
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }

        public IReadOnlyDictionary<BodyTag, IBodyPart> BodyParts { get; }

        public bool TryGrantBodyPart(BodyTag tag, IBodyPart bodyPart);
        public bool TryRemoveBodyPart(BodyTag tag);

    }

    [DefaultExecutionOrder(BodySystemExcutionOrder.MAIN_ORDER)]
    [AddComponentMenu("StudioScor/BodySystem/BodySystem Component", order: 0)]
    public class BodySystemComponent : BaseMonoBehaviour, IBodySystem
    {
        #region Events
        public delegate void ChangedBodyPartHandler(BodySystemComponent bodySystem, BodyTag body, IBodyPart bodyPart);
        #endregion

        [Header(" [ Body System ] ")]
        private readonly Dictionary<BodyTag, IBodyPart> _BodyParts = new();
        public IReadOnlyDictionary<BodyTag, IBodyPart> BodyParts => _BodyParts;

        public event ChangedBodyPartHandler OnGrantedBodyPart;
        public event ChangedBodyPartHandler OnRemovedBodyPart;

        private void Awake()
        {
            Setup();
        }
        private void Setup()
        {
        }

        protected virtual void OnSetup() { }

        public bool TryGrantBodyPart(BodyTag body, IBodyPart transform)
        {
            if(_BodyParts.TryAdd(body, transform))
            {
                Callback_OnGrantedBodyPart(body, transform);

                return true;
            }
            else
            {
                return false;
            }
        }
        public bool TryRemoveBodyPart(BodyTag body)
        {
            if (_BodyParts.TryGetValue(body, out IBodyPart value))
            {
                _BodyParts.Remove(body);

                Callback_OnRemovedBodyPart(body, value);

                return true;
            }
            else
            {
                return false;
            }
        }

        #region Events CallBack
        public void Callback_OnGrantedBodyPart(BodyTag bodyTag, IBodyPart bodyPart)
        {
            OnGrantedBodyPart?.Invoke(this, bodyTag, bodyPart);
        }
        public void Callback_OnRemovedBodyPart(BodyTag bodyTag, IBodyPart bodyPart)
        {
            OnRemovedBodyPart?.Invoke(this, bodyTag, bodyPart);
        }
        #endregion
    }

}
