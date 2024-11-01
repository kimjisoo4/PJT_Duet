using PF.PJT.Duet.Controller.Enemy;
using PF.PJT.Duet.Pawn;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class OrkWorriorBossRoomController : BaseMonoBehaviour
    {
        [Header(" [ Ork Worrior Encounter Controller ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private CharacterListVariable _bossCharacterVariable;
        [SerializeField] private GameObjectListVariable _activeUIInputVariable;
        [SerializeField] private GameObjectListVariable _inActiveStatusUIVariable;

        [Header(" Trigger Area ")]
        [SerializeField] private GameObject _triggerAreaActor;

        [Header(" Doors ")]
        [SerializeField] private GameObject[] _doorActors;

        [Header(" Timeline ")]
        [SerializeField] private GameObject _timelinePlayerActor;
        [SerializeField] private GameObject _orkWorriorActor;
        [SerializeField] private GameObject _orkWorriorControllerActor;
        [SerializeField] private GameObject _cinematicViewActor;
        [SerializeField] private GameObject _cinemachineCameraActor;

        [Header(" Events ")]
        [SerializeField] private GameEvent _onStageClear;

        private const string TRACK_CHARACTER_ANIMATION = "CharacterAnimationTrack";

        private IPawnSystem _pawnSystem;
        private ICharacter _character;
        private IControllerSystem _controllerSystem;
        private IEnemyController _enemyController;

        private IDoor[] _doors;
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

            _doors = new IDoor[_doorActors.Length];

            for (int i = 0; i < _doorActors.Length; i++)
            {
                var door = _doorActors[i].GetComponent<IDoor>();

                _doors[i] = door;
            }

            _character.OnSpawned += _character_OnSpawned;
            _character.OnDead += _character_OnDead;

            _timelinePlayer.OnBeforePlayed += _timelinePlayer_OnBeforePlayed;
            _timelinePlayer.OnStopped += _timelinePlayer_OnStopped;
            _triggerArea.OnEnteredTrigger += _triggerArea_OnEnteredTrigger;

            _orkWorriorActor.SetActive(false);
            _orkWorriorControllerActor.SetActive(false);
            _cinematicViewActor.SetActive(false);
            _cinemachineCameraActor.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_character is not null)
            {
                _character.OnSpawned -= _character_OnSpawned;
                _character.OnDead -= _character_OnDead;
                _character = null;
            }

            if(_timelinePlayer is not null)
            {
                _timelinePlayer.OnBeforePlayed -= _timelinePlayer_OnBeforePlayed;
                _timelinePlayer.OnStopped -= _timelinePlayer_OnStopped;
                _timelinePlayer = null;
            }

            if(_triggerArea is not null)
            {
                _triggerArea.OnEnteredTrigger -= _triggerArea_OnEnteredTrigger;
                _triggerArea = null;
            }

            if(_activeUIInputVariable)
            {
                _activeUIInputVariable.Remove(gameObject);
            }    

            if(_inActiveStatusUIVariable)
            {
                _inActiveStatusUIVariable.Remove(gameObject);
            }
        }

        private void OnEnteredPlayer()
        {
            OnPlayTimeline();
            ClossAllDoor();
        }
        private void OnClearedEnemy()
        {
            _onStageClear.Invoke();
            OpenAllDoor();
        }

        [ContextMenu(nameof(OnPlayTimeline), false, 1000000)]
        private void OnPlayTimeline()
        {
            if (_timelinePlayer.IsPlaying)
                return;

            _timelinePlayer.Play();
            _activeUIInputVariable.Add(gameObject);
            _inActiveStatusUIVariable.Add(gameObject);
        }
        private void OnBeforePlayTimeline()
        {
            _orkWorriorActor.SetActive(true);

            var playableAsset = _timelinePlayer.PlayableDirector.playableAsset;

            foreach (var output in playableAsset.outputs)
            {
                if (output.streamName == TRACK_CHARACTER_ANIMATION)
                {
                    _timelinePlayer.PlayableDirector.SetGenericBinding(output.sourceObject, _character.Model.GetComponent<Animator>());
                    break;
                }
            }
        }

        private void OpenAllDoor()
        {
            foreach (var door in _doors)
            {
                door.TryOpen();
            }
        }
        private void ClossAllDoor()
        {
            foreach (var door in _doors)
            {
                door.TryClose();
            }
        }

        private void OnWorriroController()
        {
            _orkWorriorControllerActor.SetActive(true);

            _controllerSystem.Possess(_pawnSystem);
            _enemyController.SetTargetKey(_playerManager.PlayerPawn.transform);

            _activeUIInputVariable.Remove(gameObject);
            _inActiveStatusUIVariable.Remove(gameObject);

            _character.OnSpawn();
        }

        private void _character_OnSpawned(ICharacter character)
        {
            _bossCharacterVariable.Add(_character);
        }
        private void _character_OnDead(ICharacter character)
        {
            _bossCharacterVariable.Remove(_character);

            OnClearedEnemy();
        }
        private void _timelinePlayer_OnBeforePlayed(ITimelinePlayer timelinePlayer)
        {
            OnBeforePlayTimeline();
        }

        private void _timelinePlayer_OnStopped(ITimelinePlayer timelinePlayer)
        {
            OnWorriroController();
        }
        private void _triggerArea_OnEnteredTrigger(ITriggerArea triggerArea, Collider collider)
        {
            OnEnteredPlayer();
            
        }
    }
}
