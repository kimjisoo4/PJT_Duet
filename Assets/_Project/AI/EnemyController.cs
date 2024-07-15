using BehaviorDesigner.Runtime;
using PF.PJT.Duet.Pawn;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    public class EnemyController : BaseMonoBehaviour
    {
        [Header(" [ Enemy Controller ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private BehaviorTree _behaviorTree;

        private ICharacter _character;
        private IControllerSystem _controllerSystem;
        private IDamageableSystem _damageableSytstemInPawn;
        public IControllerSystem ControllerSystem => _controllerSystem;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_behaviorTree)
            {
                _behaviorTree = gameObject.GetComponent<BehaviorTree>();
            }
#endif
        }
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
            _behaviorTree.DisableBehavior(false);
        }

        private void LateUpdate()
        {
            if (!_controllerSystem.IsPossess)
                return;

            transform.SetPositionAndRotation(_controllerSystem.Pawn.transform.position, _controllerSystem.Pawn.transform.rotation);
        }

        private const string SELF_KEY = "Self";
        private const string TARGET_KEY = "Target";
        
        private SharedTransform _selfKey;
        private SharedTransform _targetKey;

        private void SetCharacter(ICharacter newCharacter)
        {
            if (_character is not null)
            {
                _damageableSytstemInPawn.OnAfterDamage -= DamageableSystem_OnAfterDamage;
                _damageableSytstemInPawn = null;

                _behaviorTree.DisableBehavior(false);
            }
                
            _character = newCharacter;

            if (_character is not null)
            {
                _damageableSytstemInPawn = _character.gameObject.GetComponent<IDamageableSystem>();
                _damageableSytstemInPawn.OnAfterDamage += DamageableSystem_OnAfterDamage;

                _behaviorTree.EnableBehavior();

                _selfKey = (SharedTransform)_behaviorTree.GetVariable(SELF_KEY);
                
                if(_selfKey is null)
                {
                    _selfKey = new SharedTransform();

                    _behaviorTree.SetVariable(SELF_KEY, _selfKey);
                }

                _selfKey.SetValue(_controllerSystem.Pawn.transform);

                _targetKey = (SharedTransform)_behaviorTree.GetVariable(TARGET_KEY);

                if(_targetKey is null)
                {
                    _targetKey = new SharedTransform();

                    _behaviorTree.SetVariable(TARGET_KEY, _targetKey);
                }
            }
        }

        public void SetTargetKey(Transform target)
        {
            if (!_behaviorTree.isActiveAndEnabled)
                return;

            if(target.TryGetActor(out IActor actor))
            {
                target = actor.transform;
            }

            _controllerSystem.SetLookTarget(target);
            _targetKey.SetValue(target);
        }

        private void DamageableSystem_OnAfterDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
        {
            if (!_controllerSystem.IsPossess)
            {
                Log($"Un Possessed");
                return;
            }

            if (!_behaviorTree.isActiveAndEnabled)
            {
                Log($"Behavior Tree Is Disable");
                return;
            }

            if (_targetKey is null || _targetKey.Value)
            {
                return;
            }

            var damageCauser = damageInfo.Causer;
            var instigator = damageInfo.Instigator;

            if(instigator.TryGetPawn(out IPawnSystem pawn))
            {
                SetTargetKey(pawn.transform);
            }
            else if (damageCauser.TryGetActor(out IActor causerActor))
            {
                SetTargetKey(causerActor.transform);
            }
            else
            {
                SetTargetKey(damageCauser.transform);
            }
            
        }

        private void _playerManager_OnChangedPlayerPawn(PlayerManager playerManager, IPawnSystem currentPawn, IPawnSystem prevPawn = null)
        {
            if (!_controllerSystem.IsPossess)
                return;

            if (!_behaviorTree.isActiveAndEnabled)
                return;

            if (_targetKey is null)
                return;

            var target = _targetKey.Value;

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