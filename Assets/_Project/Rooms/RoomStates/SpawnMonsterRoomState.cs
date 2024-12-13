using PF.PJT.Duet.CreateCharacterSystem;
using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet
{
    public class SpawnMonsterRoomState : RoomState
    {
        [Header(" [ Spawn Monster State ] ")]
        [SerializeField] private CharacterInformationData[] _spawnEnemies;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private int _spawnCount = 5;

        private int _remainSpawnedCount;


        private void Awake()
        {
            foreach (var spawnEnemy in _spawnEnemies)
            {
                spawnEnemy.Pool.Initialization();
            }
        }

        protected override void EnterState()
        {
            base.EnterState();

            OnSpawnEnemys();
        }

        private void OnSpawnEnemys()
        {
            for (int i = 0; i < _spawnCount; i++)
            {
                var spawnPoint = _spawnPoints.ElementAtOrDefault(i);

                if (!spawnPoint)
                {
                    Log($" Not Has Spawn Point", SUtility.STRING_COLOR_FAIL);
                    break;
                }

                _remainSpawnedCount++;

                var enemyData = new CharacterData(_spawnEnemies[Random.Range(0, _spawnEnemies.Length)]);
                var character = enemyData.CreateCharacter();

                character.Teleport(spawnPoint.position, spawnPoint.rotation);

                character.OnSpawn();

                character.OnDead += Character_OnDead;
            }
        }
        public void OnClearedEnemys()
        {
            RoomController.TryNextState(this);
        }


        private void Character_OnDead(ICharacter character)
        {
            character.OnDead -= Character_OnDead;

            _remainSpawnedCount--;

            if (_remainSpawnedCount <= 0)
            {
                OnClearedEnemys();
            }
        }
    }

}
