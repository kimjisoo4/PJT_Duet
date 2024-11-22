using PF.PJT.Duet.Pawn;
using StudioScor.Utilities;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet
{
    public class SpawnMonsterRoomState : RoomState
    {
        [Header(" [ Spawn Monster State ] ")]
        [SerializeField] private PoolContainer[] _spawnEnemyPools;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private int _spawnCount = 5;

        private int _remainSpawnedCount;


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
