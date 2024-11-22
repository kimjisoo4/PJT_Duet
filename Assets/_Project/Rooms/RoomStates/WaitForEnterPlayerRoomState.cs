using StudioScor.Utilities;
using UnityEngine;


namespace PF.PJT.Duet
{
    public class WaitForEnterPlayerRoomState : RoomState
    {
        [Header(" [ Wait For Enter Player State] ")]
        [SerializeField] private GameObject _triggerAreaActor;

        private ITriggerArea _triggerArea;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            base.OnValidate();

            if(!_triggerAreaActor)
            {
                if(gameObject.TryGetComponentInParentOrChildren(out ITriggerArea triggerArea))
                {
                    _triggerAreaActor = triggerArea.gameObject;
                }
            }
#endif
        }

        protected override void EnterState()
        {
            base.EnterState();

            _triggerArea = _triggerAreaActor.GetComponent<ITriggerArea>();

            _triggerArea.OnEnteredTrigger += _triggerArea_OnEnteredTrigger;

            if(!_triggerArea.gameObject.activeSelf)
            {
                _triggerArea.gameObject.SetActive(true);
            }
        }

        protected override void ExitState()
        {
            base.ExitState();
            
            _triggerArea.OnEnteredTrigger -= _triggerArea_OnEnteredTrigger;

            if (_triggerArea.gameObject.activeSelf)
            {
                _triggerArea.gameObject.SetActive(false);
            }
        }

        private void _triggerArea_OnEnteredTrigger(ITriggerArea triggerArea, Collider collider)
        {
            RoomController.TryNextState(this);
        }
    }

}
