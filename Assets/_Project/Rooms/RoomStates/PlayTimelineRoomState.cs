using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class PlayTimelineRoomState : RoomState
    {
        [Header(" [ Play Timeline State ] ")]
        [SerializeField] private GameObject _timelinePlayerActor;

        [Header(" Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput = EBlockInputState.UI | EBlockInputState.Game;

        [Header(" HUD ")]
        [SerializeField] private HUDVisibilityController _hudVisibility;

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

            if (!_inputBlocker)
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();

            if (!_hudVisibility)
                _hudVisibility = SUtility.FindAssetByType<HUDVisibilityController>();
#endif
        }

        private void Awake()
        {
            _timelinePlayer = _timelinePlayerActor.GetComponent<ITimelinePlayer>();
        }

        protected override void EnterState()
        {
            base.EnterState();

            _inputBlocker.BlockInput(this, _blockInput);
            _hudVisibility.RequestHide(this);

            _timelinePlayer.OnStopped += _timelinePlayer_OnStopped;
            _timelinePlayer.Play();
        }

        protected override void ExitState()
        {
            base.ExitState();

            _inputBlocker.UnblockInput(this);
            _hudVisibility.RequestShow(this);

            _timelinePlayer.Stop();
        }

        private void _timelinePlayer_OnStopped(ITimelinePlayer timelinePlayer)
        {
            RoomController.ForceNextState(this);
        }
    }
}
