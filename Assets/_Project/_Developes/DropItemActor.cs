using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class DropItemActor : BaseMonoBehaviour
    {
        [Header(" [ Drop Item Actor ] ")]
        [SerializeField] private EquipmentItem _equipmentItem;
        [SerializeField] private GameObject _triggerAreaActor;

        private IEquipment _equipment;
        private ITriggerArea _triggerArea;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!_triggerAreaActor)
            {
                if(gameObject.TryGetComponentInChildren(out ITriggerArea triggerArea))
                {
                    _triggerAreaActor = triggerArea.gameObject;
                }
            }
        }
#endif
        private void Awake()
        {
            if (_triggerAreaActor)
            {
                _triggerArea = _triggerAreaActor.GetComponent<ITriggerArea>();
            }
            else
            {
                _triggerArea = _triggerAreaActor.gameObject.GetComponentInChildren<ITriggerArea>();
                _triggerAreaActor = _triggerArea.gameObject;
            }

            _triggerArea.OnEnteredTrigger += _triggerArea_OnEnteredTrigger;
        }
        private void OnDestroy()
        {
            if(_triggerArea is not null)
            {
                _triggerArea.OnEnteredTrigger -= _triggerArea_OnEnteredTrigger;
            }
        }

        private void _triggerArea_OnEnteredTrigger(ITriggerArea triggerArea, Collider collider)
        {
            if(collider.gameObject.TryGetComponent(out IEquipmentWearer equipmentWearer))
            {
                if(_equipment is null)
                    _equipment = _equipmentItem.CreateSpec();

                if(equipmentWearer.TryEquipItem(_equipment))
                {
                    _equipment = null;
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
