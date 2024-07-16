using UnityEngine;

namespace StudioScor.Utilities
{
    [CreateAssetMenu(menuName = "StudioScor/Utilities/Animation/AnimNotifyStateEvent/new Log", fileName = "AnimNotifyState_Log_")]
    public class AnimNotifyStateEvent_Log : AnimNotifyStateEvent
    {
        [Header(" [ Anim Notify State Event Log] ")]
        [SerializeField] private string _enterLog;
        [SerializeField] private string _exitLog;
        public override void Enter(AnimNotifyComponent animNotifyComponent)
        {
            Log(_enterLog);
        }

        public override void Exit(AnimNotifyComponent animNotifyComponent)
        {
            Log(_exitLog);
        }
    }
}