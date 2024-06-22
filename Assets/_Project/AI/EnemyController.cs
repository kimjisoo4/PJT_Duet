using PF.PJT.Duet.Pawn;
using StudioScor.PlayerSystem;
using StudioScor.StateMachine;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    public class EnemyController : BaseMonoBehaviour
    {
        [Header(" [ Enemy Controller ] ")]
        [SerializeField] private PlayerManager _playerManager;

        [Header(" State Machine ")]
        [SerializeField] private StateMachineComponent _stateMachine;
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private BlackBoardKey_Transform _targetKey;

        private ICharacter _character;
        private IControllerSystem _controllerSystem;
        private IDamageableSystem _damageableSytstemInPawn;
        public IControllerSystem ControllerSystem => _controllerSystem;

        private void Awake()
        {
            _controllerSystem = gameObject.GetControllerSystem();

            _playerManager.OnChangedPlayerPawn += _playerManager_OnChangedPlayerPawn;
            _controllerSystem.OnPossessedPawn += _controllerSystem_OnPossessedPawn;
            _controllerSystem.OnUnPossessedPawn += _controllerSystem_OnUnPossessedPawn;
        }

        private void OnDestroy()
        {
            if(_playerManager)
            {
                _playerManager.OnChangedPlayerPawn -= _playerManager_OnChangedPlayerPawn;
            }

            if(_controllerSystem is null)
            {
                _controllerSystem.OnPossessedPawn -= _controllerSystem_OnPossessedPawn;
                _controllerSystem.OnUnPossessedPawn -= _controllerSystem_OnUnPossessedPawn;
            }
        }

        private void OnEnable()
        {
            if(ControllerSystem.IsPossess)
            {
                var character = ControllerSystem.Pawn.gameObject.GetComponent<Character>();

                SetCharacter(character);
            }
        }
        private void OnDisable()
        {
            _stateMachine.EndStateMachine();
        }

        private void LateUpdate()
        {
            if (!_controllerSystem.IsPossess)
                return;

            transform.SetPositionAndRotation(_controllerSystem.Pawn.transform.position, _controllerSystem.Pawn.transform.rotation);
        }

        private void SetCharacter(ICharacter newCharacter)
        {
            if (_character is not null)
            {
                _damageableSytstemInPawn.OnAfterDamage -= DamageableSystem_OnAfterDamage;
                _damageableSytstemInPawn = null;
                _stateMachine.EndStateMachine();
            }
                
            _character = newCharacter;

            if (_character is not null)
            {
                _damageableSytstemInPawn = _character.gameObject.GetComponent<IDamageableSystem>();
                _damageableSytstemInPawn.OnAfterDamage += DamageableSystem_OnAfterDamage;

                _stateMachine.OnStateMachine();
                _selfKey.SetValue(_stateMachine, ControllerSystem);
            }
        }

        private void SetTargetKey(Transform target)
        {
            if (!_stateMachine.IsPlaying)
                return;

            if(target)
            {
                _controllerSystem.SetLookTarget(target);
                _targetKey.SetValue(_stateMachine, target);
            }
            else
            {
                _controllerSystem.SetLookTarget(null);
                _targetKey.Clear(_stateMachine);
            }
        }

        private void DamageableSystem_OnAfterDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
        {
            if (!_controllerSystem.IsPossess)
                return;

            if (!_stateMachine.IsPlaying)
                return;

            if (_targetKey.HasValue(_stateMachine))
                return;

            var damageCauser = damageInfo.Causer;

            if (!damageCauser.TryGetActor(out IActor actor))
            {
                SetTargetKey(damageCauser ? damageCauser.transform : null);
            }
            else if(actor.gameObject.TryGetPawnSystem(out IPawnSystem pawn))
            {
                SetTargetKey(pawn.transform);
            }
            else
            {
                SetTargetKey(actor.transform);
            }
        }

        private void _playerManager_OnChangedPlayerPawn(PlayerManager playerManager, IPawnSystem currentPawn, IPawnSystem prevPawn = null)
        {
            if (!_stateMachine.IsPlaying)
                return;

            if (!_targetKey.TryGetValue(_stateMachine, out Transform target))
                return;

            if (prevPawn is null || target != prevPawn.transform)
            {
                Log($"Target Not Equal [ Target - {target} :: Prev Pawn - {prevPawn.transform}]");

                return;
            }

            SetTargetKey(currentPawn.transform);
        }

        private void _controllerSystem_OnPossessedPawn(IControllerSystem controller, IPawnSystem pawn)
        {
            var character = pawn.gameObject.GetComponent<ICharacter>();

            SetCharacter(character);
        }

        private void _controllerSystem_OnUnPossessedPawn(IControllerSystem controller, IPawnSystem pawn)
        {
            SetCharacter(null);
        }

        
    }
}