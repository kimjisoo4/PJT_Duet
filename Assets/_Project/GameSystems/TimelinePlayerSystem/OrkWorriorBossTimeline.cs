using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class OrkWorriorBossTimeline : TimelinePlayer
    {
        [Header(" [ Ork Worrior Boss Timeline ] ")]
        [SerializeField] private GameObject _orkWorriorActor;
        [SerializeField] private GameObject _orkWorriorControllerActor;
        [SerializeField] private GameObject _cinematicViewActor;
        [SerializeField] private GameObject _cinemachineCameraActor;

        [Header(" Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput = EBlockInputState.UI | EBlockInputState.Game;
        
        private const string TRACK_CHARACTER_ANIMATION = "CharacterAnimationTrack";
        private ICharacter _character;

        protected override void OnValidate()
        {
            base.OnValidate();

#if UNITY_EDITOR
            if(!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }
#endif
        }
        protected override void Awake()
        {
            base.Awake();

            _character = _orkWorriorActor.GetComponent<ICharacter>();

            _orkWorriorActor.SetActive(false);
            _orkWorriorControllerActor.SetActive(false);
            _cinematicViewActor.SetActive(false);
            _cinemachineCameraActor.SetActive(false);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            _inputBlocker.UnblockInput(this);
        }

        protected override void OnBeforePlay()
        {
            _inputBlocker.BlockInput(this, _blockInput);

            _orkWorriorActor.SetActive(true);

            var playableAsset = PlayableDirector.playableAsset;

            foreach (var output in playableAsset.outputs)
            {
                if (output.streamName == TRACK_CHARACTER_ANIMATION)
                {
                    PlayableDirector.SetGenericBinding(output.sourceObject, _character.Model.GetComponent<Animator>());
                    break;
                }
            }
        }
        protected override void OnStop()
        {
            base.OnStop();

            _inputBlocker.UnblockInput(this);
        }
    }
}
