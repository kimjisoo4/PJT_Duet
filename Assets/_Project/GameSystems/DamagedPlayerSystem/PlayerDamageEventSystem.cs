using PF.PJT.Duet.Controller;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class PlayerDamageEventSystem : BaseMonoBehaviour
    {
        [Header(" [ Player Damage Event System ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private GameplayTagSO _onHitTriggerTag;
        [SerializeField] private PlayerDamageEventModifier[] _modifiers;

        private IPlayerController _playerController;
        private IGameplayTagSystem _mainCharacterTagSystem;
        private IGameplayTagSystem _subCharacterTagSystem;

        private void Awake()
        {
            if (_playerManager.HasPlayerController)
            {
                _playerController = _playerManager.PlayerController.gameObject.GetComponent<PlayerController>();
            }

            _playerManager.OnChangedPlayerController += _playerManager_OnChangedPlayerController;   
        }
        private void OnDestroy()
        {
            if(_playerManager)
            {
                _playerManager.OnChangedPlayerController -= _playerManager_OnChangedPlayerController;
            }
        }

        private void _playerManager_OnChangedPlayerController(PlayerManager playerManager, IControllerSystem currentController, IControllerSystem prevController = null)
        {
            if(_playerController is not null)
            {
                if (_mainCharacterTagSystem is not null)
                {
                    _mainCharacterTagSystem.OnTriggeredTag -= _mainCharacterTagSystem_OnTriggeredTag;
                    _mainCharacterTagSystem = null;
                }

                if (_subCharacterTagSystem is not null)
                {
                    _subCharacterTagSystem.OnTriggeredTag -= _subCharacterTagSystem_OnTriggeredTag;
                    _subCharacterTagSystem = null;
                }
            }


            if (currentController is null)
            {
                _playerController = null;
            }
            else
            {
                _playerController = _playerManager.PlayerController.gameObject.GetComponent<IPlayerController>();
            }

            if(_playerController is not null)
            {
                if (_playerController.CurrentCharacter is not null)
                {
                    _mainCharacterTagSystem = _playerController.CurrentCharacter.gameObject.GetGameplayTagSystem();
                    _mainCharacterTagSystem.OnTriggeredTag += _mainCharacterTagSystem_OnTriggeredTag;
                }

                if (_playerController.NextCharacter is not null)
                {
                    _subCharacterTagSystem = _playerController.NextCharacter.gameObject.GetGameplayTagSystem();
                    _subCharacterTagSystem.OnTriggeredTag += _subCharacterTagSystem_OnTriggeredTag;
                }
            }
        }

        private void OnHitPlayerCharacter(DamageInfoData damageInfo)
        {
            foreach (var modifier in _modifiers)
            {
                modifier.OnHit(damageInfo);
            }
        }

        private void _mainCharacterTagSystem_OnTriggeredTag(IGameplayTagSystem gameplayTagSystem, IGameplayTag gameplayTag, object data = null)
        {
            if (!_playerController.CurrentCharacter.gameObject != gameplayTagSystem.gameObject)
                return;

            if (_onHitTriggerTag != gameplayTag)
                return;

            OnHitPlayerCharacter((DamageInfoData)data);
        }
        private void _subCharacterTagSystem_OnTriggeredTag(IGameplayTagSystem gameplayTagSystem, IGameplayTag gameplayTag, object data = null)
        {
            if (!_playerController.CurrentCharacter.gameObject != gameplayTagSystem.gameObject)
                return;

            if (_onHitTriggerTag != gameplayTag)
                return;

            OnHitPlayerCharacter((DamageInfoData)data);
        }
    }
}
