using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet
{
    public class EnemyRoomController : BaseMonoBehaviour
    {
        [Header(" [ Enemy Room Controller ] ")]
        [SerializeField] private GameObject[] _doorActors;
        [SerializeField] private GameObject _triggerActor;
        [SerializeField] private PoolContainer[] _spawnEnemyPools;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private int _spawnCount = 5;

        [SerializeField] private GameEvent _rewordEvent;

        private int _remainSpawnedCount;

        private IDoor[] _doors;
        private ITriggerArea _triggerArea;

        private void Awake()
        {
            _doors = new IDoor[_doorActors.Length];

            for (int i = 0; i < _doorActors.Length; i++)
            {
                var doorActor = _doorActors[i];

                _doors[i] = doorActor.GetComponent<IDoor>();
            }

            _triggerArea = _triggerActor.GetComponent<ITriggerArea>();

            _triggerArea.OnEnteredTrigger += _triggerArea_OnEnteredTrigger;
        }

        public void OnEnteredPlayer()
        {
            OnSpawnEnemys();

            CloseAllDoor();
        }

        public void OnClearedEnemys()
        {
            if(_rewordEvent)
                _rewordEvent.Invoke();

            OpenAllDoor();
        }

        private void OpenAllDoor()
        {
            foreach (var door in _doors)
            {
                door.ForceOpen();
            }
        }
        private void CloseAllDoor()
        {
            foreach (var door in _doors)
            {
                door.ForceClose();
            }
        }
        private void OnSpawnEnemys()
        {
            for(int i = 0; i < _spawnCount; i++)
            {
                var spawnPoint = _spawnPoints.ElementAtOrDefault(i);
                if(!spawnPoint)
                {
                    Log($" Not Has Spawn Point", SUtility.STRING_COLOR_FAIL);
                    break;
                }

                var spawnEnemyPool = _spawnEnemyPools[Random.Range(0, _spawnEnemyPools.Length)];
                var enemy = spawnEnemyPool.Get();


                if (enemy.TryGetComponent(out ICharacter character))
                {
                    _remainSpawnedCount++;

                    character.Teleport(spawnPoint.position, spawnPoint.rotation);
                    character.ResetCharacter();
                    character.OnSpawn();

                    character.OnDead += Character_OnDead;
                }
            }
        }

        private void Character_OnDead(ICharacter character)
        {
            character.OnDead -= Character_OnDead;

            _remainSpawnedCount--;

            if(_remainSpawnedCount <= 0)
            {
                OnClearedEnemys();
            }
        }

        private void _triggerArea_OnEnteredTrigger(ITriggerArea triggerArea, Collider collider)
        {
            _triggerArea.OnEnteredTrigger -= _triggerArea_OnEnteredTrigger;

            OnEnteredPlayer();
        }
    }

}
