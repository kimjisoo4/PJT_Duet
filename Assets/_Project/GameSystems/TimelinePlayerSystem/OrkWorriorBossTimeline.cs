using PF.PJT.Duet.Pawn;
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

        private ICharacter _character;

        private const string TRACK_CHARACTER_ANIMATION = "CharacterAnimationTrack";

        protected override void Awake()
        {
            base.Awake();

            _character = _orkWorriorActor.GetComponent<ICharacter>();

            _orkWorriorActor.SetActive(false);
            _orkWorriorControllerActor.SetActive(false);
            _cinematicViewActor.SetActive(false);
            _cinemachineCameraActor.SetActive(false);
        }

        protected override void OnBeforePlay()
        {
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
    }
}
