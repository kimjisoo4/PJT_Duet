using UnityEngine;
using StudioScor.Utilities;

namespace StudioScor.BodySystem
{
    public interface IBodyPart
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public IBodySystem BodySystem { get; }
        public BodyTag BodyTag { get; }
        public bool CanEquiped { get; }
        public void SetCanEquip(bool isEquip);
        public void SetOwner(IBodySystem newBodySystem);
    }

    [DefaultExecutionOrder(BodySystemExcutionOrder.SUB_ORDER)]
    public class BodyPartComponent : BaseMonoBehaviour, IBodyPart
    {
        [Header(" [ Body Part Component ] ")]
        [SerializeField] private BodyTag _BodyTag;
        [SerializeField] private bool _canEquiped = true;
        [SerializeField] private bool _useAutoAttach = true;

        private IBodySystem _bodySystem;
        public BodyTag BodyTag => _BodyTag;
        public IBodySystem BodySystem => _bodySystem;
        public bool CanEquiped => _canEquiped;

#if UNITY_EDITOR
        private void Reset()
        {
            _bodySystem = GetComponentInParent<IBodySystem>();
        }
#endif
        public void SetOwner(IBodySystem newBodySystem)
        {
            if(_bodySystem is not null)
            {
                Log("Try Remove Body Part - " + _BodyTag);
                BodySystem.TryRemoveBodyPart(_BodyTag);
            }

            _bodySystem = newBodySystem;

            if (_bodySystem is not null)
            {
                Log("Try Grant Body Part - " + _BodyTag);
                BodySystem.TryGrantBodyPart(_BodyTag, this);
            }
        }

        private void OnEnable()
        {
            if (!_useAutoAttach)
                return;

            if (!transform.TryGetComponentInParentOrChildren(out _bodySystem))
            {
                LogError($"{gameObject} is Not Has {nameof(IBodySystem)}!!!");
            }

            SetOwner(_bodySystem);
        }

        private void OnDisable()
        {
            if (!_useAutoAttach)
                return;

            SetOwner(null);
        }

        public void SetCanEquip(bool canEquip)
        {
            if (_canEquiped == canEquip)
                return;

            _canEquiped = canEquip;
        }
    }
}
