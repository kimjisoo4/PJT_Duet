using PF.PJT.Duet.Controller.Enemy;
using PF.PJT.Duet.Pawn;
using StudioScor.PlayerSystem;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class BossBattleRoomState : RoomState
    {
        [Header(" [ Boss Battle State ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private GameObject _bossCharacterActor;
        [SerializeField] private GameObject _bossControllerActor;

        [SerializeField] private CharacterListVariable _activatedBossCharacters;

        private IPawnSystem _pawnSystem;
        private IControllerSystem _controllerSystem;

        private ICharacter _character;
        private IEnemyController _enemyController;

        private void Awake()
        {
            _pawnSystem = _bossCharacterActor.GetPawnSystem();
            _character = _pawnSystem.gameObject.GetComponent<ICharacter>();

            _controllerSystem = _bossControllerActor.GetControllerSystem();
            _enemyController = _controllerSystem.gameObject.GetComponent<IEnemyController>();

            _bossCharacterActor.SetActive(false);
            _bossControllerActor.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_character is not null)
            {
                _character.OnSpawned -= _character_OnSpawned;
                _character.OnDead -= _character_OnDead;
                _character = null;
            }
        }

        protected override void EnterState()
        {
            base.EnterState();

            OnPossessedBoss();
        }

        private void OnPossessedBoss()
        {
            _bossControllerActor.SetActive(true);

            _controllerSystem.Possess(_pawnSystem);
            _enemyController.SetTargetKey(_playerManager.PlayerPawn.transform);

            _character.OnSpawned += _character_OnSpawned;
            _character.OnDead += _character_OnDead;

            _character.OnSpawn();
        }

        private void _character_OnSpawned(ICharacter character)
        {
            character.OnSpawned -= _character_OnSpawned;

            _activatedBossCharacters.Add(character);
        }
        private void _character_OnDead(ICharacter character)
        {
            character.OnDead -= _character_OnDead;

            _activatedBossCharacters.Remove(character);

            RoomController.ForceNextState(this);
        }
    }
}
