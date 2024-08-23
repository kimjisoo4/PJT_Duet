using PF.PJT.Duet.Controller.Enemy;
using PF.PJT.Duet.Pawn;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class OrkWorriorEncounterController : BaseMonoBehaviour
    {
        [Header(" [ Ork Worrior Encounter Controller ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private GameObjectListVariable _activeUIInputVariable;
        [Space(5f)]
        [SerializeField] private GameObject _orkWorriorActor;
        [SerializeField] private GameObject _orkWorriorControllerActor;
        [SerializeField] private GameObject _cinematicViewActor;
        [SerializeField] private GameObject _cinemachineCameraActor;
        [Space(5f)]
        [SerializeField] private GameObject _timelinePlayerActor;
        [SerializeField] private GameObject _triggerAreaActor;

        private const string TRACK_CHARACTER_ANIMATION = "CharacterAnimationTrack";
        private IPawnSystem _pawnSystem;
        private ICharacter _character;
        private IControllerSystem _controllerSystem;
        private IEnemyController _enemyController;

        private ITimelinePlayer _timelinePlayer;
        private ITriggerArea _triggerArea;

        private void Awake()
        {
            _pawnSystem = _orkWorriorActor.GetPawnSystem();
            _character = _orkWorriorActor.GetComponent<ICharacter>();
            _controllerSystem = _orkWorriorControllerActor.GetControllerSystem();
            _enemyController = _orkWorriorControllerActor.GetComponent<IEnemyController>();

            _timelinePlayer = _timelinePlayerActor.GetComponent<ITimelinePlayer>();
            _triggerArea = _triggerAreaActor.GetComponent<ITriggerArea>();

            _timelinePlayer.OnBeforePlayed += _timelinePlayer_OnBeforePlayed;
            _timelinePlayer.OnStopped += _timelinePlayer_OnStopped;
            _triggerArea.OnEnteredTrigger += _triggerArea_OnEnteredTrigger;

            _orkWorriorActor.SetActive(false);
            _orkWorriorControllerActor.SetActive(false);
            _cinematicViewActor.SetActive(false);
            _cinemachineCameraActor.SetActive(false);
        }

        [ContextMenu(nameof(OnPlayTimeline), false, 1000000)]
        private void OnPlayTimeline()
        {
            if (_timelinePlayer.IsPlaying)
                return;

            _timelinePlayer.Play();
            _activeUIInputVariable.Add(gameObject);
        }
        private void OnWorriorActivate()
        {
            _orkWorriorActor.SetActive(true);
        }
        private void OnWorriroController()
        {
            _orkWorriorControllerActor.SetActive(true);

            _controllerSystem.OnPossess(_pawnSystem);
            _enemyController.SetTargetKey(_playerManager.PlayerPawn.transform);
            _activeUIInputVariable.Remove(gameObject);
        }


        private void _timelinePlayer_OnBeforePlayed(ITimelinePlayer timelinePlayer)
        {
            OnWorriorActivate();

            var playableAsset = _timelinePlayer.PlayableDirector.playableAsset;

            foreach (var output in playableAsset.outputs)
            {
                if(output.streamName == TRACK_CHARACTER_ANIMATION)
                {
                    _timelinePlayer.PlayableDirector.SetGenericBinding(output.sourceObject, _character.Model.GetComponent<Animator>());
                    break;
                }
            }
        }

        private void _timelinePlayer_OnStopped(ITimelinePlayer timelinePlayer)
        {
            OnWorriroController();
        }
        private void _triggerArea_OnEnteredTrigger(ITriggerArea triggerArea, Collider collider)
        {
            OnPlayTimeline();
        }
    }
}
