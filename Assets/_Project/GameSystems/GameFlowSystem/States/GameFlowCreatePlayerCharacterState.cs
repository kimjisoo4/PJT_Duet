using PF.PJT.Duet.Controller;
using PF.PJT.Duet.CreateCharacterSystem;
using StudioScor.PlayerSystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{

    [AddComponentMenu("Duet/GameFlow/State/Create Player Character State")]
    public class GameFlowCreatePlayerCharacterState : GameFlowState
    {
        [Header(" [ Create Player Character State ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private CreatePlayerCharacterController _createCharacterSystem;
        [SerializeField] private int _maxCharacterCount;

        private IPlayerController _playerController;

        public override bool CanEnterState()
        {
            if (!base.CanEnterState())
                return false;
            
            if (!_playerManager)
                return false;

            if (!_playerManager.HasPlayerController)
                return false;

            if (!_playerManager.PlayerController.gameObject.TryGetComponent(out _playerController))
                return false;

            if (_playerController.Characters.Count >= _maxCharacterCount)
                return false;

            return true;
        }

        protected override void EnterState()
        {
            base.EnterState();

            _playerController.OnAddedCharacter += _playerController_OnAddedCharacter;
            _createCharacterSystem.OnInactivated += _createCharacterSystem_OnInactivated;

            _createCharacterSystem.Init();
            _createCharacterSystem.Activate();
        }

        protected override void ExitState()
        {
            base.ExitState();

            if(_playerController is not null)
            {
                _playerController.OnAddedCharacter -= _playerController_OnAddedCharacter;
            }

            if(_createCharacterSystem)
            {
                _createCharacterSystem.OnInactivated -= _createCharacterSystem_OnInactivated;
            }

            _createCharacterSystem.Inactivate();
        }

        private void _playerController_OnAddedCharacter(IPlayerController controller, Pawn.ICharacter character)
        {
            if (_playerController.Characters.Count >= _maxCharacterCount)
            {
                _createCharacterSystem.Inactivate();
            }
        }

        private void _createCharacterSystem_OnInactivated(CreatePlayerCharacterController createPlayerCharacterSystem)
        {
            TryNextState();
        }
    }
}
