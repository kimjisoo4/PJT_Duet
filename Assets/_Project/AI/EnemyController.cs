﻿using PF.PJT.Duet.Pawn;
using StudioScor.BodySystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using Unity.Behavior;
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
        [SerializeField] private PooledObject _pooledObject;
        [SerializeField] private BehaviorGraphAgent _behaviorAgent;

        [Header(" Gameplay Tags ")]
        [SerializeField] private GameplayTag _stiffenTag;
        [SerializeField] private GameplayTag _groggyTag;

        [Header(" Follow Target  ")]
        [SerializeField] private BodyTag _headTag;

        private const string PAWN_KEY = "Pawn";
        private const string TARGET_KEY = "Target";
        private const string ORIGIN_POSITION_KEY = "StartPosition";
        private const string STIFFEN_KEY = "Stiffen";
        private const string GROGGY_KEY = "Groggy";

        private BlackboardVariable<GameObject> _targetVariable;
        private BlackboardVariable<bool> _stiffenVariable;
        private BlackboardVariable<bool> _groggyVariable;

        private ISightSensor _sightSensor;
        private ICharacter _character;
        private IRelationshipSystem _relationshipSystem;
        private IControllerSystem _controllerSystem;
        private IDamageableSystem _damageableSytstemInPawn;
        private IGameplayTagSystem _gameplayTagSystemInPawn;
        public IControllerSystem ControllerSystem => _controllerSystem;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_pooledObject)
            {
                _pooledObject = gameObject.GetComponent<PooledObject>();
            }
            if(!_behaviorAgent)
            {
                _behaviorAgent = gameObject.GetComponent<BehaviorGraphAgent>();
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
            if(_controllerSystem.IsPossessed)
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

                _gameplayTagSystemInPawn.OnGrantedOwnedTag -= _gameplayTagSystemInPawn_OnGrantedOwnedTag;
                _gameplayTagSystemInPawn.OnRemovedOwnedTag -= _gameplayTagSystemInPawn_OnRemovedOwnedTag;
                _gameplayTagSystemInPawn = null;

                _behaviorAgent.End();

                _sightSensor.RemoveIgnoreTransform(_character.transform);
            }

            _character = newCharacter;

            if (_character is not null)
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);

                _damageableSytstemInPawn = _character.gameObject.GetComponent<IDamageableSystem>();
                _damageableSytstemInPawn.OnAfterDamage += DamageableSystem_OnAfterDamage;

                _relationshipSystem = _character.gameObject.GetRelationshipSystem();

                _behaviorAgent.Restart();

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

                if(_character.gameObject.TryGetGameplayTagSystem(out _gameplayTagSystemInPawn))
                {
                    _gameplayTagSystemInPawn.OnGrantedOwnedTag += _gameplayTagSystemInPawn_OnGrantedOwnedTag;
                    _gameplayTagSystemInPawn.OnRemovedOwnedTag += _gameplayTagSystemInPawn_OnRemovedOwnedTag;
                }

                SetupBlackboard();

                _sightSensor.OnSight();
            }
            else
            {
                _sightSensor.EndSight();
                _behaviorAgent.End();

                _pooledObject.Release();
            }
        }



        private void _gameplayTagSystemInPawn_OnRemovedOwnedTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag)
        {
            if (!_behaviorAgent.Graph.IsRunning)
                return;

            if (_stiffenTag == gameplayTag)
            {
                _stiffenVariable.Value = false;
            }
            else if (_groggyTag == gameplayTag)
            {
                _groggyVariable.Value = false;
            }
        }

        private void _gameplayTagSystemInPawn_OnGrantedOwnedTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag)
        {
            if (!_behaviorAgent.Graph.IsRunning)
                return;

            if (_stiffenTag == gameplayTag)
            {
                _stiffenVariable.Value = true;
            }
            else if (_groggyTag == gameplayTag)
            {
                _groggyVariable.Value = true;
            }
        }

        public void SetTargetKey(Transform target)
        {
            if (!_behaviorAgent.Graph.IsRunning)
            {
                Log($"Behavior Graph Is Disable");
                return;
            }

            if (target.TryGetActor(out IActor actor))
            {
                target = actor.transform;
            }

            _controllerSystem.SetLookTarget(target);
            _targetVariable.Value = target.gameObject;

            if (target)
                _sightSensor.EndSight();
            else
                _sightSensor.OnSight();
        }

        private void SetupBlackboard()
        {
            _behaviorAgent.SetVariableValue<Vector3>(ORIGIN_POSITION_KEY, _controllerSystem.Pawn.transform.position);
            _behaviorAgent.SetVariableValue<GameObject>(PAWN_KEY, _controllerSystem.Pawn.gameObject);

            _behaviorAgent.BlackboardReference.GetVariable(TARGET_KEY, out _targetVariable);
            _behaviorAgent.BlackboardReference.GetVariable(STIFFEN_KEY, out _stiffenVariable);
            _behaviorAgent.BlackboardReference.GetVariable(GROGGY_KEY, out _groggyVariable);

            _targetVariable.Value = null;
            _stiffenVariable.Value = false;
            _groggyVariable.Value = false;
        }

        private void DamageableSystem_OnAfterDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
        {
            if (!_controllerSystem.IsPossessed)
            {
                Log($"Un Possessed");
                return;
            }

            if (!_behaviorAgent.Graph.IsRunning)
            { 
                Log($"Behavior Graph Is Disable");
                return;
            }

            var damageCauser = damageInfo.Causer;
            var instigator = damageInfo.Instigator;

            if(instigator.TryGetPawn(out IPawnSystem pawn))
            {
                if(pawn.IsPlayer)
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
            if (!_controllerSystem.IsPossessed)
                return;

            if (!_behaviorAgent.Graph.IsRunning)
            {
                Log($"Behavior Graph Is Disable");
                return;
            }

            var target = _targetVariable.Value;

            if(!target)
            {
                Log($" Prev Target Is Null");
                return;
            }

            if (prevPawn is null)
            {
                Log($" Prev Pawn Is Null");
                return;
            }

            if (target != prevPawn.gameObject)
            {
                Log($"Target Not Equal [ Target - {target} :: Prev Pawn - {prevPawn.gameObject}]");

                return;
            }

            SetTargetKey(currentPawn.transform);
        }

        private void _sightSensor_OnFoundSight(ISightSensor sightSensor, ISightTarget sight)
        {
            if (!_controllerSystem.IsPossessed)
                return;

            if (!_behaviorAgent.Graph.IsRunning)
            {
                Log($"Behavior Graph Is Disable");
                return;
            }

            if (_targetVariable.Value)
                return;

            if(sight.gameObject.TryGetPawn(out IPawnSystem pawn) && pawn.gameObject.TryGetReleationshipSystem(out IRelationshipSystem relationSystem))
            {
                var relationship = _relationshipSystem.CheckRelationship(relationSystem);

                if (relationship == ERelationship.Hostile)
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