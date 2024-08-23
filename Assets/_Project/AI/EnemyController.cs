using BehaviorDesigner.Runtime;
using PF.PJT.Duet.Pawn;
using StudioScor.BodySystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    public interface IEnemyController
    {
        public void SetTargetKey(Transform target);
    }

    public class EnemyController : BaseMonoBehaviour, IEnemyController
    {
        [Header(" [ Enemy Controller ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private BehaviorTree _behaviorTree;
        [SerializeField] private ExternalBehavior _externalBehavior;

        [Header(" Follow Target  ")]
        [SerializeField] private BodyTag _headTag;

        private const string SELF_KEY = "Self";
        private const string TARGET_KEY = "Target";
        private const string ORIGIN_POSITION_KEY = "OriginPos";

        private SharedTransform _selfKey;
        private SharedTransform _targetKey;
        private SharedVector3 _originPosKey;
        private ISightSensor _sightSensor;
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
            _sightSensor = gameObject.GetComponentInChildren<ISightSensor>();

            _playerManager.OnChangedPlayerPawn += _playerManager_OnChangedPlayerPawn;
            _controllerSystem.OnPossessedPawn += _controllerSystem_OnPossessedPawn;
            _controllerSystem.OnUnPossessedPawn += _controllerSystem_OnUnPossessedPawn;

            _sightSensor.OnFoundSight += _sightSensor_OnFoundSight;
        }
        private void OnDestroy()
        {
            if(_playerManager)
            {
                _playerManager.OnChangedPlayerPawn -= _playerManager_OnChangedPlayerPawn;
            }

            if(_controllerSystem is not null)
            {
                _controllerSystem.OnPossessedPawn -= _controllerSystem_OnPossessedPawn;
                _controllerSystem.OnUnPossessedPawn -= _controllerSystem_OnUnPossessedPawn;
            }

            if(_sightSensor is not null)
            {
                _sightSensor.OnFoundSight -= _sightSensor_OnFoundSight;
            }
        }

        private void FixedUpdate()
        {
            if(_controllerSystem.IsPossess)
            {
                float deltaTime = Time.fixedDeltaTime;

                _sightSensor.UpdateSight(deltaTime);
            }
        }

        private void SetCharacter(ICharacter newCharacter)
        {
            if (_character is not null)
            {
                _damageableSytstemInPawn.OnAfterDamage -= DamageableSystem_OnAfterDamage;
                _damageableSytstemInPawn = null;

                _behaviorTree.ExternalBehavior = null;
                _behaviorTree.DisableBehavior(false);

                _sightSensor.RemoveIgnoreTransform(_character.transform);
            }

            _character = newCharacter;

            if (_character is not null)
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);

                _damageableSytstemInPawn = _character.gameObject.GetComponent<IDamageableSystem>();
                _damageableSytstemInPawn.OnAfterDamage += DamageableSystem_OnAfterDamage;

                _behaviorTree.ExternalBehavior = _externalBehavior;
                _behaviorTree.EnableBehavior();

                _sightSensor.AddIgnoreTransform(_character.transform);
                if (_character.gameObject.TryGetBodySystem(out IBodySystem bodySystem))
                {
                    if (bodySystem.TryGetBodyPart(_headTag, out IBodyPart bodypart))
                    {
                        _sightSensor.SetOwner(bodypart.gameObject);
                    }
                    else
                    {
                        _sightSensor.SetOwner(bodySystem.gameObject);
                    }
                }

                SetupBlackboard();

                _sightSensor.OnSight();
            }
            else
            {
                _sightSensor.EndSight();
                _behaviorTree.DisableBehavior(false);

                gameObject.SetActive(false);
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

            if (target)
                _sightSensor.EndSight();
            else
                _sightSensor.OnSight();
        }

        private void SetupBlackboard()
        {
            _selfKey = (SharedTransform)_behaviorTree.GetVariable(SELF_KEY);

            if (_selfKey is null)
            {
                _selfKey = new SharedTransform();

                _behaviorTree.SetVariable(SELF_KEY, _selfKey);
            }

            _selfKey.SetValue(_controllerSystem.Pawn.transform);



            _targetKey = (SharedTransform)_behaviorTree.GetVariable(TARGET_KEY);

            if (_targetKey is null)
            {
                _targetKey = new SharedTransform();

                _behaviorTree.SetVariable(TARGET_KEY, _targetKey);
            }



            _originPosKey = (SharedVector3)_behaviorTree.GetVariable(ORIGIN_POSITION_KEY);

            if (_originPosKey is null)
            {
                _originPosKey = new SharedVector3();

                _behaviorTree.SetVariable(ORIGIN_POSITION_KEY, _originPosKey);
            }

            _originPosKey.SetValue(_controllerSystem.Pawn.transform.position);
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

            if(prevPawn is null)
            {
                Log($" Prev Pawn Is Null");
                return;
            }

            if (target != prevPawn.transform)
            {
                Log($"Target Not Equal [ Target - {target} :: Prev Pawn - {prevPawn.transform}]");

                return;
            }

            SetTargetKey(currentPawn.transform);
        }

        private void _sightSensor_OnFoundSight(ISightSensor sightSensor, ISightTarget sight)
        {
            if (!_controllerSystem.IsPossess)
                return;

            if (!_behaviorTree.isActiveAndEnabled)
                return;

            if (_targetKey is null)
                return;

            if (_targetKey.Value)
                return;

            if(sight.gameObject.TryGetPawn(out IPawnSystem pawn))
            {
                var affiliation = pawn.Controller.CheckAffiliation(_controllerSystem);

                if (affiliation == EAffiliation.Hostile)
                {
                    SetTargetKey(pawn.transform);
                }
            }
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