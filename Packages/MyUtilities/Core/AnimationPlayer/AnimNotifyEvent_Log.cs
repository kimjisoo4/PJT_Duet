using UnityEngine;

namespace StudioScor.Utilities
{
    [CreateAssetMenu(menuName ="StudioScor/Utilities/Animation/AnimNotifyEvent/new Log", fileName ="AnimNotify_Log_")]
    public class AnimNotifyEvent_Log : AnimNotifyEvent
    {
        [Header(" [ Anim Notify Event Log] ")]
        [SerializeField] private string _log;
        public override void Enter(AnimNotifyComponent animNotifyComponent)
        {
            Log(_log);
        }
    }
}