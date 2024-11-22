using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class PlayTimelineRoomState : RoomState
    {
        [Header(" [ Play Timeline State ] ")]
        [SerializeField] private GameObject _timelinePlayerActor;

        private ITimelinePlayer _timelinePlayer;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            base.OnValidate();

            if(!_timelinePlayerActor)
            {
                if(gameObject.TryGetComponentInChildren(out ITimelinePlayer timelinePlayer))
                {
                    _timelinePlayerActor = timelinePlayer.gameObject;
                }
            }
#endif
        }

        private void Awake()
        {
            _timelinePlayer = _timelinePlayerActor.GetComponent<ITimelinePlayer>();
        }

        protected override void EnterState()
        {
            base.EnterState();

            _timelinePlayer.OnStopped += _timelinePlayer_OnStopped;
            _timelinePlayer.Play();
        }

        protected override void ExitState()
        {
            base.ExitState();

            _timelinePlayer.Stop();
        }

        private void _timelinePlayer_OnStopped(ITimelinePlayer timelinePlayer)
        {
            RoomController.ForceNextState(this);
        }
    }
}
