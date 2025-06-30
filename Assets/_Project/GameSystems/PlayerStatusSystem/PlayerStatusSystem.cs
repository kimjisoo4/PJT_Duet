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
        [SerializeField] private GameObject _statusUIActor;
        [SerializeField] private StatusUIToSimpleAmount[] _statusBars;

        private bool _isActivate;
        public bool IsActivate => _isActivate;

        private GameObject _character;

        private void Awake()
        {
            UpdateVisible();
        }
        private void Start()
        {
            if(_playerManager.HasPlayerPawn)
            {
                SetCharacter(_playerManager.PlayerPawn.gameObject);
            }

            _playerManager.OnChangedPlayerPawn += PlayerManager_OnChangedPlayerPawn;
        }

        private void OnDestroy()
        {
            if(_playerManager)
            {
                _playerManager.OnChangedPlayerPawn -= PlayerManager_OnChangedPlayerPawn;
            }
        }

        public void SetCharacter(GameObject target)
        {
            _character = target;

            for (int i = 0; i < _statusBars.Length; i++)
            {
                var bar = _statusBars[i];

                bar.SetTarget(_character);
            }
        }

        public void Activate()
        {
            if (_isActivate)
                return;

            _isActivate = true;

            UpdateVisible();
        }
        public void Deactivate()
        {
            if (!_isActivate)
                return;

            _isActivate = false;

            UpdateVisible();
        }
        private void UpdateVisible()
        {
            if (_character)
            {
                _statusUIActor.SetActive(true);
            }
            else
            {
                _statusUIActor.SetActive(false);
            }
        }


        private void PlayerManager_OnChangedPlayerPawn(PlayerManager playerManager, IPawnSystem currentPawn, IPawnSystem prevPawn = null)
        {
            var target = currentPawn is null ? null : currentPawn.gameObject;

            SetCharacter(target);
        }
    }
}
