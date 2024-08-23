using StudioScor.PlayerSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class PlayerStatusSystem : BaseMonoBehaviour
    {
        [Header(" [ Player Status System ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private StatusUIToSimpleAmount[] _statusBars;
        private void Start()
        {
            if(_playerManager.HasPlayerPawn)
            {
                SetCharacter(_playerManager.PlayerPawn.gameObject);
            }

            _playerManager.OnChangedPlayerPawn += _playerManager_OnChangedPlayerPawn;
        }
        private void OnDestroy()
        {
            if(_playerManager)
            {
                _playerManager.OnChangedPlayerPawn -= _playerManager_OnChangedPlayerPawn;
            }
        }

        public void SetCharacter(GameObject character)
        {
            for(int i = 0; i < _statusBars.Length; i++)
            {
                var bar = _statusBars[i];
            
                bar.SetTarget(character);
            }
        }

        private void _playerManager_OnChangedPlayerPawn(PlayerManager playerManager, IPawnSystem currentPawn, IPawnSystem prevPawn = null)
        {
            var target = currentPawn is null ? null : currentPawn.gameObject;

            SetCharacter(target);
        }

        
    }
}
