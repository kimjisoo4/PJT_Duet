using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    [AddComponentMenu("Duet/Door/State/Door Actor State")]
    public class DoorActorState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<DoorActorState> { }

        [Header(" [ Door State ] ")]
        [SerializeField] private StateMachineableDoorActor _doorActor;

        public StateMachineableDoorActor DoorActor => _doorActor;

        protected virtual void OnValidate()
        {
            if(!_doorActor)
            {
                _doorActor = gameObject.GetComponentInParent<StateMachineableDoorActor>();
            }
        }
    }

}
